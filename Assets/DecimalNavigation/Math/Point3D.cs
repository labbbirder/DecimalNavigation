using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using scalar = FixMath.NET.Fix64;

namespace DecimalNavigation
{


    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Point3D : IEquatable<Point3D>
    {
        [FieldOffset(0)]
        public scalar X;
        [FieldOffset(scalar.SIZE_IN_BYTES)]
        public scalar Y;
        [FieldOffset(scalar.SIZE_IN_BYTES * 2)]
        public scalar Z;

        public static readonly Point3D Zero;
        static Point3D()
        {
            Zero = new(0, 0, 0);
        }

        public Point2D XZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(X, Z);
        }

        public scalar Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => scalar.Sqrt(Magnitude2);
        }

        public readonly scalar Magnitude2 => X * X + Y * Y + Z * Z;

        public Point3D Normalized
        {
            get
            {
                var mag2 = Magnitude2;
                if (mag2 == 0)
                {
                    return Zero;
                }
                else
                {
                    return this / scalar.Sqrt(mag2);
                }
            }
        }

        public Point3D(scalar x, scalar y, scalar z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            // magnitude2 = x * x + y * y + z * z;
        }

        public Point3D(Vector3 v)
        {
            X = (scalar)v.x;
            Y = (scalar)v.y;
            Z = (scalar)v.z;
            // magnitude2 = x * x + y * y + z * z;
        }

        public static Point3D operator -(Point3D l)
        {
            return new Point3D(-l.X, -l.Y, -l.Z);
        }

        public static Point3D operator -(Point3D l, Point3D r)
        {
            return new Point3D(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3D operator +(in Point3D l, in Point3D r)
        {
            return new Point3D(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3D operator *(in Point3D p, scalar m)
        {
            return new Point3D(p.X * m, p.Y * m, p.Z * m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3D operator /(in Point3D p, scalar m)
        {
            m = scalar.One / m;
            return p * m;
        }

        public static Point3D operator *(scalar m, Point3D p)
        {
            return p * m;
        }

        public static Point3D Min(in Point3D l, in Point3D r)
        {
            return new(scalar.Min(l.X, r.X), scalar.Min(l.Y, r.Y), scalar.Min(l.Z, r.Z));
        }

        public static Point3D Max(in Point3D l, in Point3D r)
        {
            return new(scalar.Max(l.X, r.X), scalar.Max(l.Y, r.Y), scalar.Max(l.Z, r.Z));
        }

        public static Point3D Cross(Point3D l, Point3D r)
        {
            /*
                | lx rx i |
            Det | ly ry j |
                | lz rz k |
             */
            return new Point3D(
                l.Y * r.Z - l.Z * r.Y,
                l.Z * r.X - l.X * r.Z,
                l.X * r.Y - l.Y * r.X
            );
        }

        public static scalar Dot(Point3D l, Point3D r)
        {
            return l.X * r.X + l.Y * r.Y + l.Z * r.Z;
        }
        public override string ToString()
        {
            return "Vector3:" + X + "," + Y + "," + Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public static implicit operator Vector3(Point3D p) => p.ToVector3();
        public static explicit operator Point3D(Vector3 p) => new(p);

        public bool Equals(Point3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
    }
}