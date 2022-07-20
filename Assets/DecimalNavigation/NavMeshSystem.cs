//author: bbbirder
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace DecimalNavigation
{

    class FixedMath
    {
#pragma warning disable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int cob(float v)
        {
            return -~(int)(*(uint*)&v << 2 >> 25);
        }
#pragma warning restore
        /// <summary>
        /// 快速整数平方根
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static long Sqrt(long n)
        {
            if (n <= 0) return 0;

            long x = 2 << cob(n) / 2;
            //int x = 15;
            //while (-~x * -~x < n || ~-x * ~-x > n)
            while (x * x > n)
            {
                x = (x + n / x) >> 1;
            }
            return x;
        }
        /// <summary>
        /// 判断线是否相交
        /// </summary>
        /// <param name="a">线1x</param>
        /// <param name="b">线1y</param>
        /// <param name="c">线2x</param>
        /// <param name="d">线2y</param>
        /// <returns></returns>
        public static bool IsLineCross(Point2D a, Point2D b, Point2D c, Point2D d)
        {
            //var minx1 = Math.Min(a.x, b.x);
            //var maxx1 = Math.Max(a.x, b.x);
            //var miny1 = Math.Min(a.y, b.y);
            //var minx2 = Math.Min(c.x, d.x);
            //var miny2 = Math.Min(c.y, d.y);
            //if(ma)
            //UnityEngine.Debug.Log(Point2D.Cross(a - c, b - c) + "," + Point2D.Cross(a - d, b - d));
            return Math.Sign(Point2D.Cross(a - c, b - c)) * Math.Sign(Point2D.Cross(a - d, b - d)) < 0 &&
            Math.Sign(Point2D.Cross(c - a, d - a)) * Math.Sign(Point2D.Cross(c - b, d - b)) < 0;
        }


        /// <summary>
        /// 判断点是否在三角形内,包括边
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsInTriangle(Point2D a, Point2D b, Point2D c, Point2D p)
        {

            var pa = a - p;
            var pb = b - p;
            var pc = c - p;

            if (
                (pa.x > 0 && pb.x > 0 && pc.x > 0) ||
                (pa.x < 0 && pb.x < 0 && pc.x < 0) ||
                (pa.y > 0 && pb.y > 0 && pc.y > 0) ||
                (pa.y < 0 && pb.y < 0 && pc.y < 0)
            ) return false;

            var c_ab = System.Math.Sign(Point2D.Cross(pa, pb));
            var c_bc = System.Math.Sign(Point2D.Cross(pb, pc));
            var c_ca = System.Math.Sign(Point2D.Cross(pc, pa));

            return c_ab * c_bc >= 0 && c_bc * c_ca >= 0;
        }


        public static bool IsInTriangle_XZ(Point3D a, Point3D b, Point3D c, Point3D p)
        {
            return IsInTriangle(a.xz, b.xz, c.xz, p.xz);
        }
        public static Point2D NearestPointInLine(Point2D a, Point2D b, Point2D p)
        {
            var ap = p - a;
            var ab = b - a;
            var l = ab.magnitude;
            var r = Point2D.Dot(ab, ap) / l;
            if (r <= 0)
            {
                return a;
            }
            else if (r >= l)
            {
                return b;
            }
            else
            {
                return a + ab * (int)r / (int)l;
            }
        }
        public static Point3D NearestPointInLine(Point3D a, Point3D b, Point3D p)
        {
            var ap = p - a;
            var ab = b - a;
            var l = ab.magnitude;
            var r = Point3D.Dot(ab, ap) / l;
            if (r <= 0)
            {
                return a;
            }
            else if (r >= l)
            {
                return b;
            }
            else
            {
                return a + ab * (int)r / (int)l;
            }
        }

    }
    [Serializable]
    public struct Point2D : IEquatable<Point2D>
    {
        public long x, y;
        public long magnitude => FixedMath.Sqrt(x * x + y * y);

        public Point2D(long x, long y)
        {
            this.x = x;
            this.y = y;
        }
        public Point2D(UnityEngine.Vector2 v)
        {
            x = (int)v.x;
            y = (int)v.y;
        }
        public static Point2D operator -(Point2D l)
        {
            return new Point2D(-l.x, -l.y);
        }
        public static Point2D operator -(Point2D l, Point2D r)
        {
            return new Point2D(l.x - r.x, l.y - r.y);
        }
        public static Point2D operator +(Point2D l, Point2D r)
        {
            return new Point2D(l.x + r.x, l.y + r.y);
        }
        public static Point2D operator *(Point2D p, int m)
        {
            return new Point2D(p.x * m, p.y * m);
        }
        public static Point2D operator /(Point2D p, int m)
        {
            return new Point2D(p.x / m, p.y / m);
        }
        public static Point2D operator *(int m, Point2D p)
        {
            return p * m;
        }
        public static long Cross(Point2D l, Point2D r)
        {
            return l.x * r.y - r.x * l.y;
        }
        public static long Cross_XZ(Point3D l, Point3D r)
        {
            return l.x * r.z - r.x * l.z;
        }
        public static long Dot(Point2D l, Point2D r)
        {
            return l.x * r.x + l.y * r.y;
        }
        public override string ToString()
        {
            return "Vector2:" + x + "," + y;
        }

        public bool Equals(Point2D other)
        {
            return x == other.x && y == other.y;
        }
    }
    [Serializable]
    public struct Point3D : IEquatable<Point3D>
    {
        public long x, y, z;
        public Point2D xz => new Point2D(x, z);
        public long magnitude => FixedMath.Sqrt(x * x + y * y + z * z);
        public long magnitude2 => x * x + y * y + z * z;
        public Point3D(long x, long y, long z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Point3D(Vector3 v)
        {
            x = (int)v.x;
            y = (int)v.y;
            z = (int)v.z;
        }
        public static Point3D operator -(Point3D l)
        {
            return new Point3D(-l.x, -l.y, -l.z);
        }
        public static Point3D operator -(Point3D l, Point3D r)
        {
            return new Point3D(l.x - r.x, l.y - r.y, l.z - r.z);
        }
        public static Point3D operator +(Point3D l, Point3D r)
        {
            return new Point3D(l.x + r.x, l.y + r.y, l.z + r.z);
        }
        public static Point3D operator *(Point3D p, int m)
        {
            return new Point3D(p.x * m, p.y * m, p.z * m);
        }
        public static Point3D operator /(Point3D p, int m)
        {
            return new Point3D(p.x / m, p.y / m, p.z / m);
        }
        public static Point3D operator *(Point3D p, long m)
        {
            return new Point3D(p.x * m, p.y * m, p.z * m);
        }
        public static Point3D operator /(Point3D p, long m)
        {
            return new Point3D(p.x / m, p.y / m, p.z / m);
        }
        public static Point3D operator *(int m, Point3D p)
        {
            return p * m;
        }
        public static Point3D Cross(Point3D l, Point3D r)
        {
            /*
                | lx rx i |
            Det | ly ry j |
                | lz rz k |
             */
            return new Point3D(
                l.y * r.z - l.z * r.y,
                l.z * r.x - l.x * r.z,
                l.x * r.y - l.y * r.x
            );
        }
        public static long Dot(Point3D l, Point3D r)
        {
            return l.x * r.x + l.y * r.y + l.z * r.z;
        }
        public override string ToString()
        {
            return "Vector3:" + x + "," + y + "," + z;
        }
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public bool Equals(Point3D other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }


    class LineIndice
    {
        public int ia;
        public int ib;

        public LineIndice(int a, int b)
        {
            ia = a; ib = b;
        }
        public override bool Equals(object other) =>
            (ia == ((LineIndice)other).ia && ib == ((LineIndice)other).ib) ||
        (ia == ((LineIndice)other).ib && ib == ((LineIndice)other).ia);

        public override int GetHashCode()
        {
            return ia * ib;
        }
        //public static bool operator ==(LineIndice l, LineIndice r) => l.ia == r.ia && l.ib == r.ib;
        //public static bool operator !=(LineIndice l, LineIndice r) => l.ia != r.ia || l.ib != r.ib;
    }


    public unsafe struct AStarNode:IEquatable<AStarNode>,IComparable<AStarNode>
    {
        public int prevIndex;
        public int index;
        public int HValue;
        public int GValue;
        public int FValue
        {
            get
            {
                return GValue + HValue;
            }
        }
        public Point3D A;
        public Point3D B;
        public Point3D C;
        //public readonly Point3D ab;
        //public Point3D ba => A - B;
        //public Point3D ac => C - A;
        //public Point3D ca => A - C;
        //public Point3D cb => B - C;
        //public Point3D bc => C - B;
        public Point3D center { get => (A + B + C) / 3; }
        public FixedList32Bytes<int> surrounds;

        public static AStarNode Create(Point3D A, Point3D B, Point3D C)
        {
            var node = new AStarNode();
            node.A = A;
            node.B = B;
            node.C = C;
            node.surrounds = new();
            return node;
        }

        public int GetDistance(Point3D p)
        {
            return (int)(center - p).magnitude;
        }

        public int GetDistance(AStarNode n)
        {
            return GetDistance(n.center);
        }

        public FixedList32Bytes<int> GetSurrounds()
        {
            return surrounds;
        }

        public bool IsInside(Point3D p)
        {
            return FixedMath.IsInTriangle_XZ(A, B, C, p);
        }

        public Vector3 ToVector3()
        {
            return center.ToVector3();
        }

        public bool Equals(AStarNode other)
        {
            return index == other.index;
        }

        public int CompareTo(AStarNode other)
        {
            return FValue - other.FValue;
        }
    }


    public class NavigationSystem
    {
        public static int precision = 100;
        public Point3D[] vertices;
        public int[] indices;
        public int[] edges;
        public NativeArray<AStarNode> nodes;


        NativeList<Point3D> path = new();
        //AStarSystem system;

        public NavigationSystem(NormalizedNavmeshAsset asset)
        {
            nodes = new(asset.indices.Length/3,Allocator.Persistent);
            path = new(Allocator.Persistent);
            indices = asset.indices;
            vertices = asset.vertices;

            /* start of calculate edge */
            var lineCount = new Dictionary<LineIndice, int>();
            VisitTriangle((a, b, c) =>
            {
                var ab = new LineIndice(a, b);
                var bc = new LineIndice(b, c);
                var ca = new LineIndice(a, c);
                if (!lineCount.ContainsKey(ab)) lineCount[ab] = 0;
                if (!lineCount.ContainsKey(bc)) lineCount[bc] = 0;
                if (!lineCount.ContainsKey(ca)) lineCount[ca] = 0;
                lineCount[ab] += 1;
                lineCount[bc] += 1;
                lineCount[ca] += 1;
            });
            var lstEdges = new List<int>();
            foreach (var item in lineCount)
            {
                if (item.Value == 1)//is edge
                {
                    lstEdges.Add(item.Key.ia);
                    lstEdges.Add(item.Key.ib);
                }
            }
            /* end of calculate edge */

            edges = lstEdges.ToArray();
            CreateAStarNodes();
            //system = new AStarSystem(nodes);
        }

        ~NavigationSystem()
        {
            nodes.Dispose();
            path.Dispose();
        }


        void VisitTriangle(Action<int, int, int> action)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                action(indices[i + 0], indices[i + 1], indices[i + 2]);
            }
        }

        void VisitEach2Triangle(Func<int, int, int, int, int, int, bool> action, Action<int, int> action2)
        {
            for (int i = 0; i < indices.Length - 3; i += 3)
                for (int j = i + 3; j < indices.Length; j += 3)
                {
                    if (action(indices[i + 0], indices[i + 1], indices[i + 2], indices[j + 0], indices[j + 1], indices[j + 2]))
                    {
                        action2(i / 3, j / 3);
                    }
                }
        }

        public bool IsPointInMesh(Point3D p, out int triangleIndex)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                if (FixedMath.IsInTriangle_XZ(vertices[indices[i + 0]], vertices[indices[i + 1]], vertices[indices[i + 2]], p))
                {
                    triangleIndex = i;
                    return true;
                }
            }
            triangleIndex = -1;
            return false;
        }


        public bool IsPointInMesh(Point3D p)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                if (FixedMath.IsInTriangle_XZ(vertices[indices[i + 0]], vertices[indices[i + 1]], vertices[indices[i + 2]], p))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsLineInsideMesh(Point3D a, Point3D b)
        {
            for (int i = 0; i < edges.Length; i += 2)
            {
                if (FixedMath.IsLineCross(vertices[edges[i + 0]].xz, vertices[edges[i + 1]].xz, a.xz, b.xz))
                {
                    return false;
                }
            }
            return true;
        }


        //[BurstCompile]
        public struct CalcPathJob : IJob
        {
            public NativeArray<AStarNode> nodes;
            public Point3D pfrom;
            public Point3D pto;
            public NativeList<Point3D> result;

            public bool IsPointInMesh(Point3D p, out int triangleIndex)
            {
                for (int i = 0; i < nodes.Length; i ++)
                {
                    if (FixedMath.IsInTriangle_XZ(nodes[i].A, nodes[i].B, nodes[i].C, p))
                    {
                        triangleIndex = i;
                        return true;
                    }
                }
                triangleIndex = -1;
                return false;
            }

            void GetSharedPoints(AStarNode a, AStarNode b, ref FixedList64Bytes<Point3D> result)
            {
                var eye = a.A;
                result.Clear();
                if (a.A.Equals(b.A) || a.A.Equals(b.B) || a.A.Equals(b.C))
                {
                    result.Add(a.A);
                    eye = a.B;
                }
                if (a.B.Equals(b.A) || a.B.Equals(b.B) || a.B.Equals(b.C))
                {
                    result.Add(a.B);
                    if (eye.Equals(a.B)) eye = a.C;
                }
                if (a.C.Equals(b.A) || a.C.Equals(b.B) || a.C.Equals(b.C))
                {
                    result.Add(a.C);
                }
                var l = result[0];
                var r = result[1];
                if (Point2D.Cross_XZ(l - eye, r - eye) > 0)
                {
                    result[0] = r;
                    result[1] = l;
                }
            }
            public NativeList<AStarNode> AStarPathSearch(int indexFrom, int indexTo)
            {
                NativeList<int> openList = new(Allocator.Temp);
                NativeParallelHashSet<int> closeList = new(8, Allocator.Temp);
                NativeList<AStarNode> npath = new(Allocator.Temp);
                //nodeFrom = GetInsideNode(pfrom) ?? GetNearest(pfrom);
                //nodeTo = GetInsideNode(pto) ?? GetNearest(pto);
                var nodeFrom = nodes[indexFrom];
                nodeFrom.prevIndex = -1;
                nodes[indexFrom] = nodeFrom;
                openList.Add(nodeFrom.index);
                while (openList.Length > 0)
                {
                    NativeSortExtension.Sort(openList);
                    var nextIdx = openList[0];
                    var node = nodes[nextIdx];
                    if (nextIdx == indexTo)
                    {
                        while (-1 != node.prevIndex)
                        {
                            npath.Add(node);
                            node = nodes[node.prevIndex];
                        }
                        npath.Add(nodeFrom);
                        return npath;
                    }
                    foreach (var sidx in node.GetSurrounds())
                    {
                        if (closeList.Contains(sidx)) continue;
                        var snode = nodes[sidx];
                        if (openList.Contains(sidx))
                        {
                            var newDist = snode.GValue + snode.GetDistance(node);
                            if (node.GValue > newDist)
                            {
                                node.prevIndex = sidx;
                                node.GValue = newDist;
                                nodes[node.index] = node;
                            }
                            continue;
                        }
                        snode.GValue = node.GValue + snode.GetDistance(node);
                        snode.HValue = snode.GetDistance(nodes[indexTo]);
                        snode.prevIndex = node.index;
                        nodes[sidx] = snode;
                        openList.Add(sidx);
                    }
                    nodes[node.index] = node;
                    openList.RemoveAtSwapBack(0);
                    closeList.Add(node.index);
                }
                //Debug.Log(false);
                return npath;
            }

            public void Execute()
            {
                //TODO: out of mesh
                if (!IsPointInMesh(pfrom, out int ifrom))
                {
                    return;
                }
                if (!IsPointInMesh(pto, out int ito))
                {
                    return;
                }
                var npath = AStarPathSearch(ifrom, ito);

                var enableCornerProbing = true;
                /* start of probe corners */
                if (enableCornerProbing)
                {
                    result.Clear();
                    bool isNewEye = true;
                    Point3D e, l, r;
                    Point3D nr, nl;
                    e = pfrom;
                    l = e;
                    r = e;

                    var np = new FixedList64Bytes<Point3D>();
                    var li = npath.Length - 1;
                    var ri = npath.Length - 1;
                    //var curnode = zpath[0];
                    for (int i = npath.Length-1; i >= 0; i--)
                    {
                        if (i == 0)
                        {
                            np.Clear();
                            np.Add(pto);
                            np.Add(pto);
                        }
                        else
                        {
                            GetSharedPoints(npath[i], npath[i - 1], ref np);
                        }
                        if (isNewEye)
                        {
                            //ignore if in TRIANGLE_FAN list.
                            if (np[0].Equals(e) || np[1].Equals(e))
                            {
                                continue;
                            }

                            result.Add(e);

                            l = np[0] - e;
                            r = np[1] - e;
                            //avoid start-point-in-line issue
                            if (Point2D.Cross_XZ(l, r) == 0)
                            {
                                continue;
                            }
                            isNewEye = false;
                            continue;
                        }
                        nl = np[0] - e;
                        nr = np[1] - e;

                        var c_l_nl = Point2D.Cross_XZ(l, nl);
                        var c_nl_r = Point2D.Cross_XZ(nl, r);
                        var c_l_nr = Point2D.Cross_XZ(l, nr);
                        var c_nr_r = Point2D.Cross_XZ(nr, r);
                        
                        if (c_l_nl <= 0 && c_nl_r <= 0)
                        {
                            l = nl;
                            li = i;
                        }
                        if (c_l_nr <= 0 && c_nr_r <= 0)
                        {
                            r = nr;
                            ri = i;
                        }

                        if (c_nl_r > 0)// nl over right,find a corner
                        {
                            e = e + r;
                            i = ri;
                            isNewEye = true;
                            continue;
                        }
                        if (c_l_nr > 0)// nr over left,find a corner
                        {
                            e = e + l;
                            i = li;
                            isNewEye = true;
                            continue;
                        }
                    }
                    result.Add(pto);
                }
                /* end of probe corners */

            }
        }
        /// <summary>
        /// 计算路径
        /// </summary>
        /// <param name="pfrom">出发地</param>
        /// <param name="pto">目的地</param>
        /// <param name="ridgeCut">是否开启背脊切割（额外考虑高度信息）</param>
        /// <param name="cornerProbe">是否开启拐角探测（优化的路径）</param>
        /// <returns>折线路径点</returns>
        public Point3D[] CalculatePath(Point3D pfrom, Point3D pto, bool enableRidgeCutting = false, bool enableCornerProbing = true)
        {
            var result = new NativeList<Point3D>(Allocator.TempJob);
            CalcPathJob job = new CalcPathJob()
            {
                nodes = nodes,
                pfrom = pfrom,
                pto = pto,
                result = result
            };
            job.Schedule().Complete();
            var arr = result.ToArray();
            result.Dispose();
            return arr;
        }

        void ApplyRidgeCutting(List<Point3D> path, Point3D pfrom, Point3D pto, List<Point3D[]> crossPath)
        {
            /*
                b
             c  +  d
                a
             */
            var a = pfrom;
            var b = pto;
            if (a.Equals(b)) return;
            for (int i = 0; i < crossPath.Count; i++)
            {
                var c = crossPath[i][0];
                var d = crossPath[i][1];
                if (!FixedMath.IsLineCross(c.xz, d.xz, a.xz, b.xz)) continue;
                var cab = Point2D.Cross_XZ(c - a, b - a);
                var bad = Point2D.Cross_XZ(b - a, d - a);
                var bcd = Point2D.Cross_XZ(b - c, d - c);
                var dca = Point2D.Cross_XZ(d - c, a - c);

                if (bcd + dca == 0) continue;
                //var warp_ratio = dca / (bcd + dca);
                //var weft_ratio = cab / (bad + cab);

                var xz = a + (b - a) * dca / (bcd + dca);
                var y = c.y + (d.y - c.y) * cab / (bad + cab);
                path.Add(new Point3D(xz.x, y, xz.z));
            }
        }

        public void CreateAStarNodes()
        {
            var index = 0;
            VisitTriangle((a, b, c) =>
            {
                var node = AStarNode.Create(vertices[a], vertices[b], vertices[c]);
                node.index = index;
                nodes[index] = node;
                index++;
            });

            VisitEach2Triangle((a, b, c, d, e, f) =>
            {
                var cnt = 0;
                if (a == d)
                {
                    cnt++;
                }
                if (a == e) cnt++;
                if (a == f) cnt++;
                if (b == d) cnt++;
                if (b == e) cnt++;
                if (b == f) cnt++;
                if (c == d) cnt++;
                if (c == e) cnt++;
                if (c == f) cnt++;
                return cnt >= 2;
            }, (i, j) =>
            {
                var ni = nodes[i];
                var nj = nodes[j];
                ni.surrounds.Add(j);
                nj.surrounds.Add(i);
                nodes[i] = ni;
                nodes[j] = nj;
            });
        }
    }
}
