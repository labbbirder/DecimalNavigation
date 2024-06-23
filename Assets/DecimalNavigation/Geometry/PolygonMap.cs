using System;
using System.Collections.Generic;
using scalar = System.Int64;

namespace DecimalNavigation
{
    [Serializable]
    public unsafe class PolygonMap
    {
        public Point2D[] vertices;
        public List<Polygon> polygons;
        Dictionary<Segment, (Polygon, Polygon)> neighbors;
        DynamicTree<Polygon> dynamicTree;
        public PolygonMap(Point2D[] vertices)
        {
            neighbors = new();
            polygons = new();
            dynamicTree = new();
            this.vertices = vertices;
        }

        public Segment CreateSegmentID(Polygon polygon, int lineStart)
        {
            var a = polygon[lineStart];
            var b = polygon[lineStart + 1];
            return new Segment(a, b);
        }

        // public Polygon CreatePolygon(List<int> indices, int iStart, int count)
        // {
        //     var polygon = new Polygon(vertices, count);
        //     for (int i = 0; i < count; i++)
        //     {
        //         polygon[i] = indices[i + iStart];
        //     }
        //     return polygon;
        // }

        public void AddPolygon(Polygon polygon)
        {
            polygon.UpdateShape();
            dynamicTree.AddProxy(polygon);
            polygons.Add(polygon);

            for (int i = 0; i < polygon.Count; i++)
            {
                var seg = CreateSegmentID(polygon, i);
                if (!neighbors.TryGetValue(seg, out var pair))
                {
                    neighbors[seg] = pair = (polygon, default);
                }
                else
                {
                    if (pair.Item2 is null)
                    {
                        pair.Item2 = polygon;
                        pair.Item2.neighbors[seg] = pair.Item1;
                        pair.Item1.neighbors[seg] = pair.Item2;
                    }
                    else
                    {
                        throw new("More than two polygon shares a segment");
                    }
                }
            }
        }

        static List<Polygon> queryList;
        public bool GetNearestPoint(Point2D p, out Polygon polygon, out Point2D nearst)
        {
            queryList ??= new();
            nearst = default;
            polygon = default;

            dynamicTree.Query(queryList, p);

            if (queryList.Count == 0)
            {
                return false;
            }

            bool first = true;
            scalar minD = FMath.Zero;

            foreach (var poly in queryList)
            {
                if (GeometryManipulator.GetNearest(poly, p, out var np))
                {
                    nearst = np;
                    polygon = poly;
                    break;
                }

                var nd = (p - np).Magnitude2;
                if (first || nd < minD)
                {
                    first = false;
                    minD = nd;
                    nearst = np;
                    polygon = poly;
                }
            }
            queryList.Clear();
            return true;
        }

    }
}