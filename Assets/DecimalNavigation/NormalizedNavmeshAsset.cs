using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DecimalNavigation;
using UnityEngine.SceneManagement;
using com.bbbirder.Collections;

using scalar = FixMath.NET.Fix64;




#if UNITY_EDITOR
using UnityEditor;
#endif
public unsafe class NormalizedNavmeshAsset : ScriptableObject
{
    [Serializable]
    public struct PolygonData
    {
        public int[] indices;
        public AABB2D boundBox;
    }
    public static string outDir = "Assets/NavMeshResource";
    [Range(1, 1000)]
    public int precision = 100;

    public Point2D[] points;
    public List<PolygonData> polygons = new();
    //public int[] edges;

    /// <summary>
    /// 用户层->逻辑层，标准化数据，开发期调用
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="triangles"></param>
    [ContextMenu("重新计算")]
    public void RenormalizeNavMesh()
    {
        var rawMesh = NavMesh.CalculateTriangulation();
        Vector3[] vertices = rawMesh.vertices;
        int[] triangles = rawMesh.indices;

        var redirect = new Dictionary<int, int>();
        var polygons = new List<Polygon>();

        var buffer = ConvertVerticesToBuffer(ToV2, vertices, redirect);

        ConvertTrianglesToPolygons(buffer, triangles, redirect, polygons);

        points = buffer;
        this.polygons.Clear();
        foreach (var poly in polygons)
        {
            poly.UpdateShape();
            this.polygons.Add(new PolygonData()
            {
                boundBox = poly.WorldBoundingBox,
                indices = poly.indices,
            });
        }

        EditorUtility.SetDirty(this);
    }

    public void NormalizeMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        var granule = GetMinGranularity(mesh);

        for (int n = 0; n < vertices.Length; n++)
        {
            var v = vertices[n];
            scalar x = 0, y = 0;
            RoundToGranule(ref x, granule);
            RoundToGranule(ref y, granule);
            var p2 = new Point2D(x, y);
            // TODO: write back
        }

        Debug.Log(granule);
        static void RoundToGranule(ref scalar v, scalar granule)
        {
            scalar MIN_ERROR = (scalar)0.001m;
            var m = v / granule;
            if (scalar.Abs(m % 1) < MIN_ERROR)
            {
                m = scalar.Round(m);
                v = m * granule;
            }
        }
    }

    public scalar GetMinGranularity(Mesh mesh)
    {

        scalar EPSILON = scalar.One / 65536;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        IntervalList intervalSet = new();

        int i = 0;
        for (; i < triangles.Length;)
        {
            var a = ToV2(vertices[triangles[i++]]);
            var b = ToV2(vertices[triangles[i++]]);
            var c = ToV2(vertices[triangles[i++]]);

            var (xmin, xmax) = MinMax(a.X, b.X, c.X);
            var (ymin, ymax) = MinMax(a.Y, b.Y, c.Y);

            intervalSet.Union(xmin + EPSILON, xmax - EPSILON);
            intervalSet.Union(ymin + EPSILON, ymax - EPSILON);
        }

        var granule = intervalSet.GetMinGranularity();
        return granule;
    }

    public Point2D[] ConvertVerticesToBuffer(Func<Vector3, Point2D> v3p2, Vector3[] vertices, Dictionary<int, int> redirect)
    {
        var uniquePoints = new Dictionary<Point2D, int>();
        var cnt = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            var p2d = v3p2(vertices[i]);
            if (!uniquePoints.TryGetValue(p2d, out int idx))
            {
                uniquePoints[p2d] = cnt;
                redirect[i] = cnt;
                cnt++;
            }
            else
            {
                redirect[i] = idx;
            }
        }

        var pBuffer = new Point2D[cnt];
        foreach (var (p2d, idx) in uniquePoints)
        {
            pBuffer[idx] = p2d;
        }

        return pBuffer;
    }


    public unsafe void ConvertTrianglesToPolygons(Point2D[] vertices, int[] triangles, Dictionary<int, int> redirect, List<Polygon> outPolygons)
    {
        for (int i = 0; i < triangles.Length;)
        {
            var ia = redirect[triangles[i++]];
            var ib = redirect[triangles[i++]];
            var ic = redirect[triangles[i++]];
            var a = vertices[ia];
            var b = vertices[ib];
            var c = vertices[ic];
            if (Point2D.Cross(c - a, b - a) > 0)
            {
                var poly = new Polygon(vertices, 3);
                poly[0] = ia;
                poly[1] = ib;
                poly[2] = ic;
                outPolygons.Add(poly);
            }
        }
        GeometryManipulator.ConvertToConvexPolygons(outPolygons);
    }


    static (scalar min, scalar max) MinMax(scalar x, scalar y, scalar z)
    {
        if (x > y)
        {
            (x, y) = (y, x);
        }
        if (y > z)
        {
            (y, z) = (z, y);
        }
        if (x > y)
        {
            (x, y) = (y, x);
        }
        return (x, z);
    }

    Point2D ToV2(Vector3 v3) => new((scalar)v3.x * precision, (scalar)v3.z * precision);
    public static NormalizedNavmeshAsset GetInstanceOfCurrentScene()
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<NormalizedNavmeshAsset>(outDir + "/" + SceneManager.GetActiveScene().name + ".asset");
        //return AssetData
#else
        return default;
#endif
    }
    //private int[] calculateEdges()
    //{
    //    for (int i = 0; i < ; i++)
    //    {

    //    }
    //}

}

/*
TODO LIST:
    - 1. Vector to Point
    - 2. Triangles to Polygons
    3. Polygons to AStarNodes
    4. AStarNode Search
    5. Corner Search
    6. Nearest Point Search
*/