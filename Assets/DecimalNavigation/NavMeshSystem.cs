//author: bbbirder
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace DecimalNavigation
{

    class FixedMath
    {
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
            return Fix64.Sign(Point2D.Cross(a - c, b - c)) * Fix64.Sign(Point2D.Cross(a - d, b - d)) < 0 &&
            Fix64.Sign(Point2D.Cross(c - a, d - a)) * Fix64.Sign(Point2D.Cross(c - b, d - b)) < 0;
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
                (pa.X > 0 && pb.X > 0 && pc.X > 0) ||
                (pa.X < 0 && pb.X < 0 && pc.X < 0) ||
                (pa.Y > 0 && pb.Y > 0 && pc.Y > 0) ||
                (pa.Y < 0 && pb.Y < 0 && pc.Y < 0)
            ) return false;

            var c_ab = Fix64.Sign(Point2D.Cross(pa, pb));
            var c_bc = Fix64.Sign(Point2D.Cross(pb, pc));
            var c_ca = Fix64.Sign(Point2D.Cross(pc, pa));

            return c_ab * c_bc >= 0 && c_bc * c_ca >= 0;
        }


        public static bool IsInTriangle_XZ(Point3D a, Point3D b, Point3D c, Point3D p)
        {
            return IsInTriangle(a.XZ, b.XZ, c.XZ, p.XZ);
        }
        public static Point2D NearestPointInLine(Point2D a, Point2D b, Point2D p)
        {
            var ap = p - a;
            var ab = b - a;
            var l = ab.Magnitude;
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
            var l = ab.Magnitude;
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

    [StructLayout(LayoutKind.Explicit)]
    struct LineIndice : IEquatable<LineIndice>
    {
        [FieldOffset(0)]
        public ulong ID;
        [FieldOffset(0)]
        public int ia;
        [FieldOffset(4)]
        public int ib;

        public LineIndice(int a, int b)
        {
            ID = 0;
            if (a > b)
            {
                ia = b; ib = a;
            }
            else
            {
                ia = a; ib = b;
            }
        }

        public bool Equals(LineIndice other)
        {
            return ID == other.ID;
        }

        public override int GetHashCode()
        {
            return ia * 322 + ib;
        }
    }


    public unsafe struct AStarNode : IEquatable<AStarNode>, IComparable<AStarNode>
    {
        public int prevIndex;
        public int index;
        public int HValue;
        public int GValue;
        public int FValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GValue + HValue;
            }
        }
        public Point3D A { get; private set; }
        public Point3D B { get; private set; }
        public Point3D C { get; private set; }
        //public readonly Point3D ab;
        //public Point3D ba => A - B;
        //public Point3D ac => C - A;
        //public Point3D ca => A - C;
        //public Point3D cb => B - C;
        //public Point3D bc => C - B;
        public Point3D center { get; private set; }
        public List<int> surrounds;

        public static AStarNode Create(Point3D A, Point3D B, Point3D C)
        {
            var node = new AStarNode();
            node.A = A;
            node.B = B;
            node.C = C;
            node.center = (A + B + C) / 3;
            node.surrounds = new();
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDistance2(in Point3D p)
        {
            return (int)(center - p).Magnitude2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDistance2(in AStarNode n)
        {
            return GetDistance2(n.center);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<int> GetSurrounds()
        {
            return surrounds;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public AStarNode[] nodes;


        //AStarSystem system;

        public NavigationSystem(NormalizedNavmeshAsset asset)
        {
            nodes = new AStarNode[asset.indices.Length / 3];
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
            // nodes.Dispose();
            // path.Dispose();
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
                if (FixedMath.IsLineCross(vertices[edges[i + 0]].XZ, vertices[edges[i + 1]].XZ, a.XZ, b.XZ))
                {
                    return false;
                }
            }
            return true;
        }


        // [BurstCompile]
        public struct CalcPathJob// : IJob
        {
            public AStarNode[] nodes;
            public Point3D pfrom;
            public Point3D pto;
            // public NativeList<Point3D> result;

            public bool IsPointInMesh(Point3D p, out int triangleIndex)
            {
                for (int i = 0; i < nodes.Length; i++)
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

            unsafe void GetSharedPoints(AStarNode a, AStarNode b, Point3D* result)
            {
                var eye = a.A;
                int i = 0;
                if (a.A.Equals(b.A) || a.A.Equals(b.B) || a.A.Equals(b.C))
                {
                    result[i++] = a.A;
                    eye = a.B;
                }
                if (a.B.Equals(b.A) || a.B.Equals(b.B) || a.B.Equals(b.C))
                {
                    result[i++] = a.B;
                    if (eye.Equals(a.B)) eye = a.C;
                }
                if (a.C.Equals(b.A) || a.C.Equals(b.B) || a.C.Equals(b.C))
                {
                    result[i++] = a.C;
                }
                var l = result[0];
                var r = result[1];
                if (Point2D.Cross_XZ(l - eye, r - eye) > 0)
                {
                    result[0] = r;
                    result[1] = l;
                }
            }
            static HashSet<int> openList = new(64);
            static HashSet<int> closeList = new(32);
            static List<AStarNode> npath = new(32);
            public List<AStarNode> AStarPathSearch(int indexFrom, int indexTo)
            {
                // NativeList<int> openList = new(Allocator.Temp);
                // NativeHashSet<int> closeList = new(8, Allocator.Temp);
                // NativeList<AStarNode> npath = new(Allocator.Temp);
                openList.Clear();
                closeList.Clear();
                npath.Clear();

                //nodeFrom = GetInsideNode(pfrom) ?? GetNearest(pfrom);
                //nodeTo = GetInsideNode(pto) ?? GetNearest(pto);
                var nodeFrom = nodes[indexFrom];
                nodeFrom.prevIndex = -1;
                nodes[indexFrom] = nodeFrom;
                openList.Add(nodeFrom.index);

                while (openList.Count > 0)
                {
                    // NativeSortExtension.Sort(openList);
                    var nextIdx = openList.First();
                    ref var node = ref nodes[nextIdx];
                    if (nextIdx == indexTo)
                    {
                        Profiler.BeginSample("post ");
                        while (-1 != node.prevIndex)
                        {
                            npath.Add(node);
                            node = ref nodes[node.prevIndex];
                        }
                        npath.Add(nodeFrom);
                        Profiler.EndSample();
                        return npath;
                    }


                    Profiler.BeginSample("search surround ");
                    foreach (var sidx in node.GetSurrounds())
                    {
                        if (closeList.Contains(sidx)) continue;
                        ref var snode = ref nodes[sidx];
                        if (openList.Contains(sidx))
                        {
                            var newDist = snode.GValue + snode.GetDistance2(node);
                            if (node.GValue > newDist)
                            {
                                node.prevIndex = sidx;
                                node.GValue = newDist;
                            }
                        }
                        else
                        {
                            snode.GValue = node.GValue + snode.GetDistance2(node);
                            snode.HValue = snode.GetDistance2(nodes[indexTo]);
                            snode.prevIndex = node.index;
                            openList.Add(sidx);
                        }
                    }
                    Profiler.EndSample();
                    // openList.RemoveAtSwapBack(0);
                    openList.Remove(nextIdx);
                    closeList.Add(node.index);
                }
                //Debug.Log(false);
                return npath;
            }

            public unsafe void Execute(ref Path result)
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

                    var np = stackalloc Point3D[2];
                    // var np = new FixedList64Bytes<Point3D>();
                    var li = npath.Count - 1;
                    var ri = npath.Count - 1;
                    //var curnode = zpath[0];
                    for (int i = npath.Count - 1; i >= 0; i--)
                    {
                        if (i == 0)
                        {
                            // np.Clear();
                            np[0] =
                            np[1] = pto;
                        }
                        else
                        {
                            GetSharedPoints(npath[i], npath[i - 1], np);
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
        public void CalculatePath(Point3D pfrom, Point3D pto, ref Path path, bool enableRidgeCutting = false, bool enableCornerProbing = true)
        {
            // var result = new NativeList<Point3D>(Allocator.TempJob);
            CalcPathJob job = new CalcPathJob()
            {
                nodes = nodes,
                pfrom = pfrom,
                pto = pto,
                // result = result
            };
            job.Execute(ref path);
            // job.Schedule().Complete();
            // path.CopyFrom(result);
            // result.Dispose();
        }
        public class Path
        {
            public Point3D[] points;
            public int Count { get; private set; }

            public Path()
            {
                points = new Point3D[32];
                Count = 0;
            }

            internal void CopyFrom(NativeList<Point3D> lst)
            {
                if (Count < lst.Length)
                {
                    Array.Resize(ref points, lst.Length);
                }
                lst.AsArray().CopyTo(points);
                Count = lst.Length;
            }
            public ref Point3D this[int index]
            {
                get
                {
                    if (index >= Count) throw new IndexOutOfRangeException();
                    return ref points[index];
                }
            }
            public void Clear()
            {
                Count = 0;
            }
            public void Add(Point3D p)
            {
                if (Count >= points.Length)
                {
                    Array.Resize(ref points, points.Length << 1);
                }
                points[Count++] = p;
            }
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
                if (!FixedMath.IsLineCross(c.XZ, d.XZ, a.XZ, b.XZ)) continue;
                var cab = Point2D.Cross_XZ(c - a, b - a);
                var bad = Point2D.Cross_XZ(b - a, d - a);
                var bcd = Point2D.Cross_XZ(b - c, d - c);
                var dca = Point2D.Cross_XZ(d - c, a - c);

                if (bcd + dca == 0) continue;
                //var warp_ratio = dca / (bcd + dca);
                //var weft_ratio = cab / (bad + cab);

                var xz = a + (b - a) * dca / (bcd + dca);
                var y = c.Y + (d.Y - c.Y) * cab / (bad + cab);
                path.Add(new Point3D(xz.X, y, xz.Z));
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
