using System.Collections.Generic;
using com.bbbirder.Collections;
using DecimalNavigation;
using FixMath.NET;
using UnityEngine;
using static NormalizedNavmeshAsset;

public class Test : MonoBehaviour
{
    public Point2D[] inputs;
    public Point2D[] substracts;
    public List<Fix64> outputs;
    IntervalList range = new();
    [ContextMenu("Go")]
    public void Do()
    {
        // range.Clear();
        // foreach (var item in inputs)
        // {
        //     range.Union((FixMath.NET.Fix64)item.X, (FixMath.NET.Fix64)item.Y);
        // }
        // foreach (var item in substracts)
        // {
        //     range.Substract(item.X, item.Y);
        // }
        // outputs = range.values;
        // range.GetMinGranularity();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            range.Clear();
            for (int i = 0; i < 100000; i++)
            {
                foreach (var item in inputs)
                {
                    range.Union(item.X, item.Y);
                }
            }
        }
    }
}