//author: bbbirder
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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
        /// 整数平方根
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


    public class AStarNode
    {
        public AStarNode parent;
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
        public readonly Point3D A, B, C;
        //public readonly Point3D ab;
        //public Point3D ba => A - B;
        //public Point3D ac => C - A;
        //public Point3D ca => A - C;
        //public Point3D cb => B - C;
        //public Point3D bc => C - B;
        public readonly Point3D center;
        public List<AStarNode> surrounds = new List<AStarNode>();

        public AStarNode(Point3D A, Point3D B, Point3D C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.center = (A + B + C) / 3;
        }

        public int GetDistance(Point3D p)
        {
            return (int)(center - p).magnitude;
        }

        public int GetDistance(AStarNode n)
        {
            return GetDistance(n.center);
        }

        public List<AStarNode> GetSurround()
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
    }


    public class NavigationSystem
    {
        public static int precision = 100;
        public Point3D[] vertices;
        public int[] indices;
        public int[] edges;
        public List<AStarNode> nodes;
        //AStarSystem system;

        public NavigationSystem(NormalizedNavmeshAsset asset)
        {
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
            nodes = CreateAStarNodes();
            //system = new AStarSystem(nodes);
        }



        void VisitTriangle(Action<int, int, int> action)
        {
            Debug.Log(indices.Length);
            for (int i = 0; i < indices.Length; i += 3)
            {
                action(indices[i + 0], indices[i + 1], indices[i + 2]);
            }
        }

        void VisitEach2Triangle(Func<int, int, int, int, int, int, bool> action, Action<int, int> action2)
        {
            Debug.Log(indices.Length);
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

        Point3D[] GetSharedPoints(int a, int b, Point3D eye)
        {
            var lst = new List<int>(3);
            var a3 = a * 3;
            var b3 = b * 3;
            for (int i = a3; i < a3 + 3; i++)
            {
                for (int j = b3; j < b3 + 3; j++)
                {
                    if (indices[i] == indices[j])
                    {
                        lst.Add(indices[i]);
                        break;
                    }
                }
            }
            var e = indices[a3] ^ indices[a3 + 1] ^ indices[a3 + 2] ^ lst[0] ^ lst[1];
            eye = vertices[e];
            var l = vertices[lst[0]];
            var r = vertices[lst[1]];
            if (Point2D.Cross_XZ(l - eye, r - eye) < 0)
                return new Point3D[] { r, l };
            return new Point3D[] { l, r };
        }
        Point3D[] GetOtherPoints(int a, Point3D e)
        {
            var lst = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                if (!vertices[indices[a * 3 + i]].Equals(e))
                {
                    lst.Add(indices[a * 3 + i]);
                }
            }
            var l = vertices[lst[0]];
            var r = vertices[lst[1]];
            if (Point2D.Cross_XZ(l - e, r - e) < 0)
                return new Point3D[] { r, l };
            return new Point3D[] { l, r };
        }
        //void SetLR(ref Point3D l, ref Point3D r)
        //{
        //    if (Point2D.Cross_XZ(l, r) < 0)
        //    {
        //        var tmp = l;
        //        l = r;
        //        r = tmp;
        //    }
        //}
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


        #region AStar-Core
        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closeList = new List<AStarNode>();
        public List<AStarNode> npath = new List<AStarNode>(32);

        public List<AStarNode> AStarPathSearch(AStarNode nodeFrom, AStarNode nodeTo)
        {
            openList.Clear();
            closeList.Clear();
            //nodeFrom = GetInsideNode(pfrom) ?? GetNearest(pfrom);
            //nodeTo = GetInsideNode(pto) ?? GetNearest(pto);
            nodeFrom.parent = null;
            openList.Add(nodeFrom);
            while (openList.Count > 0)
            {
                openList.Sort((l, r) =>
                {
                    return l.FValue - r.FValue;
                });
                var node = openList[0];
                //Debug.Log(node.surrounds.Count);
                if (node == nodeTo)
                {
                    //Debug.Log(true);
                    npath.Clear();
                    while (null != node.parent)
                    {
                        npath.Insert(0, node);
                        node = node.parent;
                    }
                    npath.Insert(0, nodeFrom);
                    return npath;
                }
                foreach (var nb in node.GetSurround())
                {
                    if (closeList.Contains(nb)) continue;
                    if (openList.Contains(nb))
                    {
                        var newDist = nb.GValue + nb.GetDistance(node);
                        if (node.GValue > newDist)
                        {
                            node.parent = nb;
                            node.GValue = newDist;
                        }
                        continue;
                    }
                    nb.GValue = node.GValue + nb.GetDistance(node);
                    nb.HValue = nb.GetDistance(nodeTo);
                    nb.parent = node;
                    openList.Add(nb);
                }
                openList.RemoveAt(0);
                closeList.Add(node);
            }
            //Debug.Log(false);
            return null;
        }


        #endregion

        List<Point3D> path = new List<Point3D>();
        public List<Point3D> CalculatePath(Point3D pfrom, Point3D pto)
        {
            var ifrom = -1;
            var ito = -1;

            //TODO: out of mesh
            if (!IsPointInMesh(pfrom, out ifrom))
            {
                Debug.LogError("start point out of mesh!");
                return null;
            }
            if (!IsPointInMesh(pto, out ito))
            {
                Debug.LogError("end point out of mesh!");
                return null;
            }
            var nfrom = nodes[ifrom / 3];
            var nto = nodes[ito / 3];
            var npath = AStarPathSearch(nfrom, nto);
            //CornerProbe(npath, pfrom, pto);
            //OptimizePath(path);

            /* start of probe corners */
            {
                path.Clear();
                bool isNewEye = true;
                Point3D e, l, r;
                Point3D nr, nl;
                e = pfrom;
                l = e;
                r = e;
                var li = 0;
                var ri = 0;
                //var curnode = zpath[0];
                for (int i = 0; i < npath.Count; i++)
                {
                    Point3D[] np;
                    if (i == npath.Count - 1)
                        np = new Point3D[] { pto, pto };
                    else
                        np = GetSharedPoints(npath[i].index, npath[i + 1].index, e);

                    if (isNewEye)
                    {
                        //if (path.Contains(e))
                        //{
                        //    var eyeIdx = path.IndexOf(e);
                        //    path.RemoveRange(eyeIdx, path.Count - eyeIdx);
                        //}

                        //if (i == npath.Count - 1)
                        //{
                        //    continue;
                        //}

                        //ignore if in TRIANGLE_FAN topology.
                        if (np[0].Equals(e) || np[1].Equals(e))
                        {
                            continue;
                        }
                        path.Add(e);
                        //np = GetOtherPoints(zpath[i].index, e);
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
                    if (Point2D.Cross_XZ(l, nl) >= 0 && Point2D.Cross_XZ(nl, r) >= 0)
                    {
                        l = nl;
                        li = i;
                    }
                    if (Point2D.Cross_XZ(l, nr) >= 0 && Point2D.Cross_XZ(nr, r) >= 0)
                    {
                        r = nr;
                        ri = i;
                    }
                    if (Point2D.Cross_XZ(r, nl) > 0)// nl over right,find a corner
                    {
                        e = e + r;
                        i = ri ;
                        isNewEye = true;
                        continue;
                    }
                    if (Point2D.Cross_XZ(nr, l) > 0)// nr over left,find a corner
                    {
                        e = e + l;
                        i = li ;
                        isNewEye = true;
                        continue;
                    }
                }
                path.Add(pto);
            }
            /* end of probe corners */

            return path;
        }

        //void CornerProbe(List<AStarNode> npath, Point3D pfrom, Point3D pto)
        //void OptimizePath(List<Point3D> zpath)
        //{
        //    for (int i = zpath.Count - 3; i >= 0; i -= 3)
        //    {
        //        if (i < 0) break;
        //        if (IsLineInsideMesh(zpath[i + 2], zpath[i]))
        //        {
        //            Debug.Log("rmv path point");
        //            zpath.RemoveAt(i + 1);
        //        }
        //    }
        //}

        //public AStarNode ImageStartNode(Point3D p)
        //{
        //    int triIdx;
        //    if (IsPointInMesh(p, out triIdx))
        //    {
        //        var node = new AStarNode(p.x, p.y, p.z);
        //        node.surrounds.Add(nodes[indices[triIdx + 0]]);
        //        node.surrounds.Add(nodes[indices[triIdx + 1]]);
        //        node.surrounds.Add(nodes[indices[triIdx + 2]]);
        //        return node;
        //    }


        //    return null;
        //}
        //void DropNode(AStarNode node)
        //{
        //    if (null == node) return;
        //    foreach (var nb in node.surrounds)
        //    {
        //        nb.surrounds.Remove(node);
        //    }
        //}
        //public AStarNode ImageDestNode(Point3D p)
        //{
        //    int triIdx;
        //    if (IsPointInMesh(p, out triIdx))
        //    {
        //        var node = new AStarNode(p.x, p.y, p.z);
        //        node.surrounds.Add(nodes[indices[triIdx + 0]]);
        //        node.surrounds.Add(nodes[indices[triIdx + 1]]);
        //        node.surrounds.Add(nodes[indices[triIdx + 2]]);
        //        nodes[indices[triIdx + 0]].surrounds.Add(node);
        //        nodes[indices[triIdx + 1]].surrounds.Add(node);
        //        nodes[indices[triIdx + 2]].surrounds.Add(node);
        //        return node;
        //    }
        //    return null;
        //}

        public List<AStarNode> CreateAStarNodes()
        {
            var nodes = new List<AStarNode>(vertices.Length);
            VisitTriangle((a, b, c) =>
            {
                var node = new AStarNode(vertices[a], vertices[b], vertices[c]);
                node.index = nodes.Count;
                nodes.Add(node);
            });

            int[] ib = { 0, 0 };
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
                nodes[i].surrounds.Add(nodes[j]);
                nodes[j].surrounds.Add(nodes[i]);
            });
            return nodes;
        }
    }
}
