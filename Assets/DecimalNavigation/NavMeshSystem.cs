//author: bbbirder
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using com.bbbirder.DecimalNavigation;
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
        PolygonMap polygonMap;
        public NavigationSystem(NormalizedNavmeshAsset asset)
        {
            var vertices = asset.points;
            polygonMap = new PolygonMap(vertices);
            foreach (var polyData in asset.polygons)
            {
                var poly = new Polygon(vertices, polyData.indices);
                polygonMap.AddPolygon(poly);
            }
        }

        static HashSet<Polygon> openList = new(64);
        static int s_ctxId;
        public bool AStarPathSearch(Polygon from, Polygon to, List<Segment> result)
        {
            // Interlocked.Increment(ref s_ctxId);
            s_ctxId++;
            openList.Clear();

            // add to open list
            // remove from open list
            // iterate open list
            // add to closed list
            // is contains in open list
            // is contains in closed list

            var curNode = from;
            curNode.aStarData.prev = null;
            curNode.aStarData.MoveToOpenList(s_ctxId);
            openList.Add(curNode);
            while (openList.Count > 0)
            {
                bool first = true;
                var minH = FMath.Zero;

                if (curNode is null)
                {
                    // find min G or H
                    foreach (var n in openList)
                    {
                        var h = n.aStarData.H;
                        if (first || h < minH)
                        {
                            first = false;
                            minH = h;
                            curNode = n;
                        }
                    }
                }

                var curAStar = curNode.aStarData;
                if (curNode == to)
                {
                    // Profiler.BeginSample("post ");
                    while (curAStar.prev != null)
                    {

                        result.Add(curAStar.segment);
                        curAStar = ((Polygon)curAStar.prev).aStarData;
                    }
                    openList.Clear();
                    // npath.Add(curNode.aStarData.segment);
                    // Profiler.EndSample();
                    return true;
                }

                openList.Remove(curNode);
                curAStar.MoveToClosedList(s_ctxId);


                // Profiler.BeginSample("search surround ");
                var nextNode = default(Polygon);
                first = true;
                foreach (var (seg, nb) in curNode.neighbors)
                {
                    var nbAStar = nb.aStarData;
                    if (nbAStar.IsInClosedList(s_ctxId)) continue;
                    if (nbAStar.IsInOpenList(s_ctxId))
                    {
                        var newDist = nbAStar.G + nb.GetDistance(curNode);
                        if (curAStar.G > newDist)
                        {
                            curAStar.G = newDist;
                            curAStar.prev = nb;
                            curAStar.segment = seg;
                        }
                    }
                    else
                    {
                        nbAStar.G = curAStar.G + nb.GetDistance(curNode);
                        nbAStar.H = nb.GetDistance(to);
                        nbAStar.prev = curNode;
                        nbAStar.segment = seg;
                        // openList.Add(sidx);
                        nbAStar.MoveToOpenList(s_ctxId);
                        openList.Add(nb);
                    }
                    var h = nbAStar.H;
                    if (first || h < minH)
                    {
                        first = false;
                        minH = h;
                        nextNode = nb;
                    }

                }
                // Profiler.EndSample();
                curNode = nextNode;
                // openList.RemoveAtSwapBack(0);
            }
            return false;
        }

        public unsafe void SearchCorners(List<Segment> segments, Point2D pfrom, Point2D pto, List<Point2D> result)
        {
            int li = 0, ri = 0;
            Point2D vl = Point2D.Zero, vr = Point2D.Zero, p = pfrom;
            bool newp = true;
            result.Add(pfrom);

            for (int i = 0; i <= segments.Count; i++)
            {
                Point2D nvl = default, nvr = default;
                if (i == segments.Count)
                {
                    nvr =
                    nvl = pto - p;
                }
                else
                {
                    var npl = polygonMap.vertices[segments[i].ia];
                    var npr = polygonMap.vertices[segments[i].ib];

                    nvl = npl - p;
                    nvr = npr - p;
                }

                // result.Add((npl + npr) / 2);
                // continue;

                if (Point2D.Cross(nvr, nvl) < 0)
                {
                    (nvl, nvr) = (nvr, nvl);
                }

                if (newp)
                {
                    newp = false;
                    li = ri = i;
                    vl = nvl;
                    vr = nvr;
                    continue;
                }

                if (Point2D.Cross(nvl, vr) > 0)
                {
                    result.Add(p = p + vr);
                    newp = true;
                    i = ri;
                    continue;
                }

                if (Point2D.Cross(vl, nvr) > 0)
                {
                    result.Add(p = p + vl);
                    newp = true;
                    i = li;
                    continue;
                }

                if (Point2D.Cross(nvl, vl) >= 0)
                {
                    vl = nvl;
                    li = i;
                }

                if (Point2D.Cross(vr, nvr) >= 0)
                {
                    vr = nvr;
                    ri = i;
                }
            }
            result.Add(pto);

        }
        static List<Segment> npath = new(32);
        /// <summary>
        /// 计算路径
        /// </summary>
        /// <param name="pfrom">出发地</param>
        /// <param name="pto">目的地</param>
        /// <param name="ridgeCut">是否开启背脊切割（额外考虑高度信息）</param>
        /// <param name="cornerProbe">是否开启拐角探测（优化的路径）</param>
        /// <returns>折线路径点</returns>
        public void CalculatePath(Point2D pfrom, Point2D pto, List<Point2D> path)
        {
            if (!polygonMap.GetNearestPoint(pto, out var nodeFrom, out pto))
            {
                return;
            }

            if (!polygonMap.GetNearestPoint(pfrom, out var nodeTo, out pfrom))
            {
                return;
            }

            npath.Clear();
            var found = AStarPathSearch(nodeFrom, nodeTo, npath);


            SearchCorners(npath, pfrom, pto, path);
        }
        // public class Path
        // {
        //     public Point2D[] points;
        //     public int Count { get; private set; }

        //     public Path()
        //     {
        //         points = new Point2D[32];
        //         Count = 0;
        //     }

        //     public ref Point2D this[int index]
        //     {
        //         get
        //         {
        //             if (index >= Count) throw new IndexOutOfRangeException();
        //             return ref points[index];
        //         }
        //     }
        //     public void Clear()
        //     {
        //         Count = 0;
        //     }
        //     public void Add(Point2D p)
        //     {
        //         if (Count >= points.Length)
        //         {
        //             Array.Resize(ref points, points.Length << 1);
        //         }
        //         points[Count++] = p;
        //     }
        // }

    }
}
