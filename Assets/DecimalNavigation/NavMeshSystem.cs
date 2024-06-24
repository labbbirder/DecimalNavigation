//author: bbbirder
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FixMath.NET;
using UnityEngine;
using UnityEngine.Profiling;

namespace DecimalNavigation
{

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
            Point2D vl = Point2D.Zero,
                    vr = Point2D.Zero,
                    p = pfrom;
            bool newp = true;

            result.Add(pfrom);

            for (int i = 0; i <= segments.Count; i++)
            {
                Point2D nvl, nvr;
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

                if (nvl.Magnitude2 == 0 || nvr.Magnitude2 == 0) continue;

                // result.Add((nvl + p + nvr + p) / 2);
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

                if (Point2D.Cross(nvl, vr) >= 0)
                {
                    result.Add(p = p + vr);
                    newp = true;
                    i = ri;
                    // Debug.Log($"get right {ri}");
                    continue;
                }

                if (Point2D.Cross(vl, nvr) >= 0)
                {
                    result.Add(p = p + vl);
                    newp = true;
                    i = li;
                    // Debug.Log($"get left {li}");
                    continue;
                }

                if (Point2D.Cross(nvl, vl) >= 0)
                {
                    vl = nvl;
                    li = i;
                    // Debug.Log($"narrow left {i}");
                }

                if (Point2D.Cross(vr, nvr) >= 0)
                {
                    vr = nvr;
                    ri = i;
                    // Debug.Log($"narrow right {i}");
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
    }
}
