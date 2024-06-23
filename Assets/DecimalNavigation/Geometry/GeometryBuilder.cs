
using System;
using System.Collections.Generic;
using DecimalNavigation;
using scalar = System.Int64;

public unsafe static class GeometryManipulator
{
    readonly static scalar Epsilon = FMath.Epsilon;
    /// <summary>
    /// 将三角形列表合并为凸多边形列表
    /// </summary>
    /// <param name="triangles"></param>
    public static void ConvertToConvexPolygons(IList<Polygon> triangles)
    {
        /* 找到公共边，再判断 p1-p2-p3 和 p1'-p2'-p3' 为凸边界

              p3,p1'
               / \
              /   \
             /poly2\
         p2 +-------+ p2'
           /  poly1 |
          /_________|
        p1          p3'
        
        */
        Polygon poly1 = default, poly2 = default;
        Point2D p1, p2, p3;
        int i11 = 0, i12 = 0;
        int i21 = 0, i22 = 0;

        for (var i1 = 0; i1 < triangles.Count; i1++)
        {
            poly1 = triangles[i1];
            for (i11 = 0; i11 < poly1.Count; i11++)
            {
                var d1 = poly1[i11];
                i12 = (i11 + 1) % poly1.Count;
                var d2 = poly1[i12];

                // 寻找公共边
                var isdiagonal = false;
                var i2 = i1;
                for (; i2 < triangles.Count; i2++)
                {
                    if (i1 == i2)
                    {
                        continue;
                    }
                    poly2 = triangles[i2];

                    for (i21 = 0; i21 < poly2.Count; i21++)
                    {
                        if (d2 != poly2[i21])
                        {
                            continue;
                        }
                        i22 = (i21 + 1) % poly2.Count;
                        if (d1 != poly2[i22])
                        {
                            continue;
                        }
                        isdiagonal = true;
                        break;
                    }
                    if (isdiagonal)
                    {
                        break;
                    }
                }

                if (!isdiagonal)
                {
                    continue;
                }

                // 判断凸边界
                p1 = poly1.GetPoint(i11 - 1);
                p2 = poly1.GetPoint(i11);
                p3 = poly2.GetPoint(i22 + 1);
                if (!IsConvex(p1, p2, p3))
                {
                    continue;
                }

                p1 = poly2.GetPoint(i21 - 1);
                p2 = poly1.GetPoint(i12);
                p3 = poly1.GetPoint(i12 + 1);
                if (!IsConvex(p1, p2, p3))
                {
                    continue;
                }

                // TODO: 使用链表优化合并操作
                // 合并成新的多边形
                var newpoly = new Polygon(poly1.Vertices, poly1.Count + poly2.Count - 2);
                var k = 0;
                for (var j = i12; j != i11; j = (j + 1) % poly1.Count)
                {
                    newpoly[k] = poly1[j];
                    k++;
                }
                for (var j = i22; j != i21; j = (j + 1) % poly2.Count)
                {
                    newpoly[k] = poly2[j];
                    k++;
                }

                triangles.RemoveAt(i2);
                triangles[i1] = newpoly;
                poly1 = newpoly;
                i11 = -1;

            }
        }
    }

    public static void GetNeighbors(this IList<Polygon> polygons, IDictionary<Segment, (Polygon, Polygon)> edge2pairs)
    {

    }

    /// <summary>
    /// 判断点 p 是否在凸多边形内
    /// </summary>
    /// <param name="polygon"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool Contains(this Polygon polygon, Point2D p)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            var p1 = polygon.GetPoint(i);
            var p2 = polygon.GetPoint(i + 1);
            if (!IsConvex(p2, p, p1)) return false;
        }
        return true;
    }

    /// <summary>
    /// 获取点 p 到凸多边形的最近距离
    /// </summary>
    /// <param name="polygon"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool GetNearest(this Polygon polygon, Point2D p, out Point2D nearest)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            var p1 = polygon.GetPoint(i);
            var p2 = polygon.GetPoint(i + 1);
            if (!IsConvex(p2, p, p1))
            {
                var proj = Point2D.Dot(p - p2, p1 - p2);
                if (proj < 0) continue;
                var dist2 = (p2 - p1).Magnitude2;
                if (proj >= dist2 || dist2 <= Epsilon)
                {
                    nearest = p1;
                    return false;
                }
                nearest = p2 + (p1 - p2) * proj / dist2;
                return false;
            }
        }
        nearest = p;
        return true;
    }

    public static bool IsConvex(Point2D p1, Point2D p2, Point2D p3)
    {
        return Point2D.Cross(p3 - p1, p2 - p1) >= 0;
    }

}