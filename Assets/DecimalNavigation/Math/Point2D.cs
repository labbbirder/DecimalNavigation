using System;
using System.Runtime.InteropServices;
using scalar = FixMath.NET.Fix64;

namespace DecimalNavigation
{
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public struct Point2D : IEquatable<Point2D>
    {
        [FieldOffset(0)]
        public scalar X;
        [FieldOffset(scalar.SIZE_IN_BYTES)]
        public scalar Y;

        public static readonly Point2D Zero;
        static Point2D()
        {
            Zero = new Point2D(0, 0);
        }

        public scalar Magnitude => scalar.Sqrt(Magnitude2);
        public scalar Magnitude2 => X * X + Y * Y;
        public Point2D Normalized
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

        public Point2D(scalar x, scalar y)
        {
            this.X = x;
            this.Y = y;
        }
        public Point2D(UnityEngine.Vector2 v)
        {
            X = (scalar)v.x;
            Y = (scalar)v.y;
        }
        public static Point2D operator -(Point2D l)
        {
            return new Point2D(-l.X, -l.Y);
        }
        public static Point2D operator -(Point2D l, Point2D r)
        {
            return new Point2D(l.X - r.X, l.Y - r.Y);
        }
        public static Point2D operator +(Point2D l, Point2D r)
        {
            return new Point2D(l.X + r.X, l.Y + r.Y);
        }
        public static Point2D operator *(Point2D p, scalar m)
        {
            return new Point2D(p.X * m, p.Y * m);
        }
        public static Point2D operator /(Point2D p, scalar m)
        {
            m = scalar.One / m;
            return p * m;
        }
        public static Point2D operator *(scalar m, Point2D p)
        {
            return p * m;
        }

        public static bool operator ==(Point2D lhs, Point2D rhs)
        {
            return rhs.X == lhs.X && rhs.Y == lhs.Y;
        }
        public static bool operator !=(Point2D lhs, Point2D rhs)
        {
            return !(lhs == rhs);
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() * 233 + Y.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (obj is Point2D p)
            {
                return Equals(p);
            }
            return false;
        }

        public static Point2D Min(in Point2D l, in Point2D r)
        {
            return new(scalar.Min(l.X, r.X), scalar.Min(l.Y, r.Y));
        }
        public static Point2D Max(in Point2D l, in Point2D r)
        {
            return new(scalar.Max(l.X, r.X), scalar.Max(l.Y, r.Y));
        }
        public static scalar Cross(Point2D l, Point2D r)
        {
            return l.X * r.Y - r.X * l.Y;
        }
        public static scalar Cross_XZ(Point3D l, Point3D r)
        {
            return l.X * r.Z - r.X * l.Z;
        }
        public static scalar Dot(Point2D l, Point2D r)
        {
            return l.X * r.X + l.Y * r.Y;
        }
        public override string ToString()
        {
            return "Vector2:" + X + "," + Y;
        }

        public bool Equals(Point2D other)
        {
            return X == other.X && Y == other.Y;
        }
    }
}