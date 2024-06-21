using System;
using System.Buffers;
using DecimalNavigation;

public class Polygon
{
    public int Count { get; private set; }
    private Point2D[] Points;

    public Point2D this[int index]
    {
        get => Points[index % Count];
        set => Points[index % Count] = value;
    }

    public Point2D GetPoint(int index)
    {
        return this[index];
    }

    public Polygon(int count)
    {
        Points = new Point2D[Count = count];
    }

    public void Resize(int size)
    {
        Count = size;
        if (Points.Length >= size)
        {
            return;
        }

        var pool = ArrayPool<Point2D>.Shared;

        Array.Clear(Points, 0, Points.Length);
        pool.Return(Points);
        Points = pool.Rent(size);
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