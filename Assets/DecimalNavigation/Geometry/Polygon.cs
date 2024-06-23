using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DecimalNavigation;
using DeterministicMath;
using Jitter2.Collision;
using scalar = System.Int64;

[Serializable]
public unsafe class Polygon : IDynamicTreeProxy
{
    public class AStarData
    {
        public scalar G;
        public scalar H;
        public Polygon prev;
        public Segment segment;
        private int ctxID;
        public bool IsInOpenList(int ctx) => ctx == ctxID;
        public bool IsInClosedList(int ctx) => ctx == -ctxID;
        public void MoveToOpenList(int ctx) => ctxID = ctx;
        public void MoveToClosedList(int ctx) => ctxID = -ctx;
    }
    public readonly Dictionary<Segment, Polygon> neighbors;
    public Point2D[] Vertices { get; private set; }
    private int MAX_NUM;
    private int _cnt;
    public int Count
    {
        get => _cnt;
        private set
            => MAX_NUM = 0xFFFFFFF - 0xFFFFFFF % (_cnt = value);
    }

    [UnityEngine.SerializeField]
    internal int[] indices;
    public AABB2D WorldBoundingBox { get; private set; }
    public AStarData aStarData = new();
    public Point2D Center { get; private set; }
    public int NodePtr { get; set; }

    public int this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => indices[(index + Count) % Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => indices[(index + Count) % Count] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2D GetPoint(int index)
    {
        return Vertices[this[index]];
    }

    public Polygon(Point2D[] vertices, int count)
    {
        neighbors = new();
        this.Vertices = vertices;
        this.indices = new int[Count = count];
    }
    public Polygon(Point2D[] vertices, int[] indices)
    {
        neighbors = new();
        this.Vertices = vertices;
        this.indices = indices;

        Count = indices.Length;
    }

    public Polygon(Point2D[] vertices, IList<int> indices, int iStart, int count) : this(vertices, count)
    {
        for (int i = 0; i < count; i++)
        {
            this.indices[i] = indices[i + iStart];
        }
    }

    public void Resize(int size)
    {
        Count = size;

        if (indices != null && indices.Length >= size)
        {
            return;
        }

        var pool = ArrayPool<int>.Shared;

        if (indices != null)
        {
            Array.Clear(indices, 0, indices.Length);
            pool.Return(indices);
        }
        indices = pool.Rent(size);
    }

    public void UpdateShape()
    {
        var min = GetPoint(0);
        var max = min;
        var sum = Point2D.Zero;
        for (int i = 1; i < Count; i++)
        {
            var p = GetPoint(i);
            sum += p;
            min = Point2D.Min(p, min);
            max = Point2D.Max(p, max);
        }
        Center = sum / Count;
        WorldBoundingBox = new AABB2D()
        {
            Min = min,
            Max = max,
        };
    }

    public scalar GetDistance(Polygon other)
    {
        return (Center - other.Center).Magnitude;
    }

    // public (int, int) SearchSharedEdge(Polygon poly)
    // {
    //     var isdiagonal = false;
    //     int i, j = 0;
    //     for (i = 0; i < Count; i++)
    //     {
    //         var d1 = this[i];
    //         var d2 = this[(i + 1) % Count];
    //         for (j = 0; j < poly.Count; j++)
    //         {
    //             var d_ = poly[j];
    //             if (d2 != d_)
    //             {
    //                 continue;
    //             }
    //             d_ = poly[(j + 1) % poly.Count];
    //             if (d1 != d_)
    //             {
    //                 continue;
    //             }
    //             isdiagonal = true;
    //             break;
    //         }
    //         if (isdiagonal)
    //         {
    //             break;
    //         }
    //     }
    //     if (!isdiagonal) return (-1, -1);
    //     return (i, j);
    // }
}