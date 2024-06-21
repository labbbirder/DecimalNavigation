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
public class NormalizedNavmeshAsset : ScriptableObject
{

    public static string outDir = "Assets/NavMeshResource";
    [Range(1, 1000)]
    public int precision = 100;
    [SerializeField]
    public Point3D[] vertices;
    public int[] indices;

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

        //var precision = AStarSystem.percision;
        var ovl = new List<Point3D>();
        //var tri = new List<LineIndice>();
        //var lil = new HashSet<LineIndice>();
        var oil = new List<int>();

        /* start weld mesh */
        var rmvList = new List<int>();
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            for (int j = i + 1; j < vertices.Length; j++)
            {
                if ((vertices[i] - vertices[j]).magnitude * precision > 1) continue;
                if (rmvList.Contains(j)) continue;
                for (int k = 0; k < triangles.Length; k++)
                {
                    //if (triangles[k] > j) triangles[k] = triangles[k] - 1;
                    if (triangles[k] == j) triangles[k] = i;
                }
                rmvList.Add(j);
            }
        }
        for (int k = 0; k < triangles.Length; k++)
        {
            triangles[k] = triangles[k] - rmvList.Count(x => x < triangles[k]);
            oil.Add(triangles[k]);
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            if (rmvList.Contains(i)) continue;
            ovl.Add(new Point3D(vertices[i] * precision));
        }
        /* end of weld mesh */

        //for (int i = 0; i < triangles.Length; i += 3)
        //{
        //    //Debug.Log(triangles[i + 0] + "," + triangles[i + 1]);
        //    //oil.Add(triangles[i + 0]);
        //    //oil.Add(triangles[i + 1]);
        //    //oil.Add(triangles[i + 1]);
        //    //oil.Add(triangles[i + 2]);
        //    //oil.Add(triangles[i + 2]);
        //    //oil.Add(triangles[i + 0]);
        //    tri.Add(new LineIndice(triangles[i + 0], triangles[i + 1]));
        //    tri.Add(new LineIndice(triangles[i + 1], triangles[i + 2]));
        //    tri.Add(new LineIndice(triangles[i + 2], triangles[i + 0]));

        //}
        //foreach (var li in lil)
        //{
        //    oil.Add(li.ia);
        //    oil.Add(li.ib);
        //}

        this.vertices = ovl.ToArray();
        this.indices = oil.ToArray();
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

        OpenIntervalSet intervalSet = new();

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

    public void BuildShape(Point2D[] vertices, int[] triangles)
    {

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

    static Point2D ToV2(Vector3 v3) => new((scalar)v3.x, (scalar)v3.z);
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
    1. Vector to Point
    2. Triangles to Polygons
    3. Polygons to AStarNodes
    4. AStarNode Search
    5. Corner Search
    6. Nearest Point Search
*/