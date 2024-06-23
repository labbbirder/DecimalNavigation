using System;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;

namespace com.bbbirder.Collections
{
    /// <summary>
    /// 可快速访问的二维等分空间
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockGrid2D<T>
    {
        private T[] values;
        private int _bitsOfSize;
        private int _sizeMinusOne;
        public int halfSize { get; private set; }

        public T this[int x, int y]
        {
            get => values[IndexOf(x, y)];
            set => values[IndexOf(x, y)] = value;
        }

        public BlockGrid2D(int halfSize = 2)
        {
            EnsureHalfSize(halfSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(int x, int y)
        {
            return IndexOf(x, y, _sizeMinusOne, _bitsOfSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(int x, int y, int sizeMinusOne, int bitsOfSize)
        {
            Assert.IsTrue(Math.Abs(x) < halfSize, $"index x {x} out of range, half size is {halfSize}");
            Assert.IsTrue(Math.Abs(y) < halfSize, $"index y {y} out of range, half size is {halfSize}");
            x &= sizeMinusOne;
            y &= sizeMinusOne;
            return (y << bitsOfSize) + x;
        }

        public (int x, int y) PositionOf(int index)
        {
            var y = index >> _bitsOfSize;
            var x = index & _sizeMinusOne;
            return (x, y);
        }

        public void EnsureHalfSize(int _halfSize)
        {
            if (halfSize >= _halfSize) return;

            var exp = CollectionUtility.CeilExponent(_halfSize);
            var nhs = 1 << exp;
            var nbos = exp + 1;
            var nsmo = (1 << nbos) - 1;

            var nlen = 1 << (-~exp << 1);
            var nArr = new T[nlen];

            for (int y = -halfSize; y < halfSize; y++)
            {
                var sfrom = IndexOf(-halfSize, y);
                var dfrom = IndexOf(-halfSize, y, nsmo, nbos);
                if (y == -1)
                {
                    Array.Copy(values, sfrom, nArr, dfrom, halfSize);
                    Array.Copy(values, 0, nArr, 0, halfSize);
                }
                else if (y == halfSize - 1)
                {
                    Array.Copy(values, sfrom, nArr, dfrom, halfSize);
                    Array.Copy(values, sfrom + halfSize, nArr, IndexOf(0, -halfSize, nsmo, nbos), halfSize);
                }
                else
                {
                    Array.Copy(values, sfrom, nArr, dfrom, halfSize << 1);
                }
            }

            halfSize = nhs;
            _bitsOfSize = nbos;
            _sizeMinusOne = nsmo;
            values = nArr;
        }

        // public void Print()
        // {
        //     for (int y = 0; y < halfSize << 1; y++)
        //     {
        //         for (int x = 0; x < halfSize << 1; x++)
        //             Console.Write(values[(y * (halfSize << 1)) + x] + "\t");
        //         Console.WriteLine();
        //     }
        // }

    }
}