using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DecNavEditor
{

    [MenuItem("Tools/生成标准化寻路网格")]
    public static void CreateNavMesh()
    {
        var inst = ScriptableObject.CreateInstance<NormalizedNavmeshAsset>();
        inst.RenormalizeNavMesh();
        var outDir = NormalizedNavmeshAsset.outDir;
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        AssetDatabase.CreateAsset(inst, outDir + "/" + SceneManager.GetActiveScene().name + ".asset");
    }
}
