using System.Collections.Generic;
using UnityEngine.Assertions;

using scalar = FixMath.NET.Fix64;

namespace com.bbbirder.Collections
{
    /// <summary>
    /// 多个独立开区间的并集
    /// </summary>
    public class OpenIntervalSet
    {
        internal static readonly scalar NegativeInfinity = scalar.MinValue;
        internal static readonly scalar PositiveInfinity = scalar.MaxValue;

        public List<scalar> holes = new();

        /// <summary>
        /// 是否为开区间状态
        /// </summary>
        public bool IsOpen { get; private set; } = true;

        /// <summary>
        /// 独立的开区间个数
        /// </summary>
        public int Count => holes.Count >> 1;

        /// <summary>
        /// 获取在实数轴上排列的第<paramref name="idx"/>个开区间
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public (scalar min, scalar max) this[int idx]
            => (holes[idx << 1], holes[(idx << 1) + 1]);

        public void Union((scalar min, scalar max) openInterval)
        {
            Union(openInterval.min, openInterval.max);
        }

        /// <summary>
        /// 与一个开区间取并集
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Union(scalar min, scalar max)
        {
            /*  bool table
                            K  I  %
                (min,        1  0  0
                min),        1  1  1
                _, (bigger,  0  1 -2
                _, bigger),  0  0 -1

                [min,        1  0
                min],        0  0
                _, [bigger,  0  1
                _, bigger],  0  0

                            R  I
                (max,        0  1
                max),        0  0
                _, (bigger,  0  1
                _, bigger),  0  0

                [max,        1  0
                max],        0  0
                _, [bigger,  0  1
                _, bigger],  0  0
            */

            if (min >= max) return;

            var imin = MinIndexOf(min);
            var imax = MaxIndexOf(max);

            var modmin = imin % 2;
            var modmax = imax % 2;
            if (imin < 0) modmin--;
            if (imax < 0) modmax--;

            var plusMin = IsOpen && modmin == 1 || modmin == 0;
            var plusMax = !IsOpen && modmax == 0;
            var insertMax = IsOpen && modmax == 0 || modmax == -2;
            var insertMin = modmin == -2 || IsOpen && modmin == 1;

            if (imin < 0) imin = ~imin;
            if (imax < 0) imax = ~imax;

            if (plusMin) imin++;
            if (plusMax) imax++;

            if (imax > imin) holes.RemoveRange(imin, imax - imin);

            if (insertMax) holes.Insert(imin, max);
            if (insertMin) holes.Insert(imin, min);
        }

        /// <summary>
        /// 取反
        /// </summary>
        public void Inverse()
        {
            IsOpen ^= true;
            if (Count <= 0) return;

            if (holes[0] == NegativeInfinity)
            {
                holes.RemoveAt(0);
            }
            else
            {
                holes.Insert(0, NegativeInfinity);
            }

            var lastIdx = holes.Count - 1;
            if (holes[lastIdx] == PositiveInfinity)
            {
                holes.RemoveAt(lastIdx);
            }
            else
            {
                holes.Add(PositiveInfinity);
            }
        }

        /// <summary>
        /// 减去一个闭区间
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Substract(scalar min, scalar max)
        {
            if (min >= max) return;

            Inverse();
            Union(min, max);
            Inverse();
        }

        /// <summary>
        /// 与一个区间取交集
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Intersect(scalar min, scalar max)
        {
            if (min >= max) return;
            Inverse();
            Union(NegativeInfinity, min);
            Union(max, PositiveInfinity);
            Inverse();
        }

        public void Union(OpenIntervalSet another)
        {
            for (int i = 0; i < another.Count; i++)
            {
                Union(another[i].min, another[i].max);
            }
        }

        public void Substract(OpenIntervalSet another)
        {
            Inverse();
            for (int i = 0; i < another.Count; i++)
            {
                Union(another[i].min, another[i].max);
            }
            Inverse();
        }

        public void Intersect(OpenIntervalSet another)
        {
            Inverse();
            another.Inverse();
            for (int i = 0; i < another.Count; i++)
            {
                Union(another[i].min, another[i].max);
            }
            another.Inverse();
            Inverse();
        }

        public void ClearZeroIntervals()
        {
            Assert.IsTrue(IsOpen, "should not clear zero intervals when in closed state");

            for (int i = 0; i < Count; i++)
            {
                var r = this[i];
                if (r.min == r.max)
                {
                    holes.RemoveRange(i << 1, 2);
                }
            }
        }

        /// <summary>
        /// 判断实数是否落在区间内
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool Contains(scalar v)
        {
            var idx = FastSearch(v);
            if (idx >= 0) return !IsOpen;
            idx = ~idx;
            return idx % 2 == 1;
        }

        public void Clear()
        {
            holes.Clear();
        }

        public int MinIndexOf(scalar item)
        {
            var idx = FastSearch(item);
            while (idx > 0 && holes[idx - 1].Equals(item))
            {
                idx--;
            }
            return idx;
        }

        public int MaxIndexOf(scalar item)
        {
            var idx = FastSearch(item);
            if (idx > 0)
            {
                while (idx < holes.Count - 1 && holes[idx + 1].Equals(item))
                {
                    idx++;
                }
            }
            return idx;
        }

        private int FastSearch(scalar item)
        {
            const int BINARY_SEARCH_THRESHOLD = 50;
            var cnt = holes.Count;
            if (cnt > BINARY_SEARCH_THRESHOLD)
            {
                return holes.BinarySearch(item);
            }
            else
            {
                int i = 0;
                for (; i < cnt; i++)
                {
                    var r = holes[i];
                    if (item > r) continue;
                    if (r == item)
                    {
                        return i;
                    }
                    else
                    {
                        return ~i;
                    }
                }
                return ~i;
            }
        }
    }


    public static class OpenIntervalSetExtensions
    {
        static readonly scalar EPSILON = scalar.One / 65536;

        /// <summary>
        /// 获取最小的等分粒度。如果不存在，返回-1
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public static scalar GetMinGranularity(this OpenIntervalSet set)
        {
            /* 问题描述 in Latex
                \begin{array}{rrclcl}
                实数域R内,有集合A:\\
                &A =\bigcup\limits_{i= 1..n} {(a_i, b_i)} \\\\
                \textrm{subject to}
                    & a_i\in R,b_i\in R \\
                    & \bigvee\limits_{i,j=1..n;i\ne j} (a_i,b_i)\cap(a_j,b_j)=\oslash \\

                求可以划分A的最小正实数x,即\\
                \mathop{\arg\min}\limits_{x} 
                    & \bigvee\limits_{z\in Z} (zx-\epsilon ,zx+\epsilon)\cap \widetilde{A} \ne\oslash \\\\
                \textrm{subject to} 
                    & x>0\\
                    & \epsilon \to 0

                \end{array}
            */

            var rmax = set[^1].max;

            // init step length
            scalar step = 0;
            for (int i = 0; i < set.Count; i++)
            {
                var r = set[i];
                var l = r.max - r.min;
                if (l > step) step = l;
            }

            // fold intervals
            for (int i = 0; i < set.Count; i++)
            {
                var r = set[i];
                if (r.min > 0) break;
                set.Union(-r.max, -r.min);
            }

            // slide and grow step length
            scalar cur = 0;
            for (int i = 0; cur < rmax; i++)
            {
                cur = i * step;
                var il = set.MinIndexOf(cur - EPSILON);
                var ir = set.MaxIndexOf(cur + EPSILON);
                // out of valid range
                if (il == ir && il < 0 && il % 2 == 0)
                {
                    if (i == 0)
                    {
                        step = -1;
                        break;
                    }
                    step += (set[(~il) >> 1].max - cur) / i;
                    cur = 0;
                    i = 1;
                }
            }

            return step;
        }
    }
}



