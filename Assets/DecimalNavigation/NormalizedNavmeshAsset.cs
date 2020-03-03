using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using DecimalNavigation;
using UnityEngine.SceneManagement;

public class NormalizedNavmeshAsset : ScriptableObject
{
    static string outDir = "Assets/NavMeshResource";

    [Range(1, 100000)]
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
    void RenormalizeNavMesh()
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
    }
    
   
    //private int[] calculateEdges()
    //{
    //    for (int i = 0; i < ; i++)
    //    {

    //    }
    //}
    [MenuItem("Tools/生成标准化寻路网格")]
    public static void CreateNavMesh()
    {
        var inst = CreateInstance<NormalizedNavmeshAsset>();
        inst.RenormalizeNavMesh();
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        AssetDatabase.CreateAsset(inst, outDir + "/" + SceneManager.GetActiveScene().name + ".asset");
    }

}