using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DecNavEditor
{
    static string outDir = "Assets/NavMeshResource";

    [MenuItem("Tools/生成标准化寻路网格")]
    public static void CreateNavMesh()
    {
        var inst = ScriptableObject.CreateInstance<NormalizedNavmeshAsset>();
        inst.RenormalizeNavMesh();
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        AssetDatabase.CreateAsset(inst, outDir + "/" + SceneManager.GetActiveScene().name + ".asset");
    }
}
