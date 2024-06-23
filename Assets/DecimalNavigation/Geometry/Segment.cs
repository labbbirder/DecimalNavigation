using System;
using System.Runtime.InteropServices;

namespace DecimalNavigation
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Segment : IEquatable<Segment>
    {
        [FieldOffset(0)]
        public ulong ID;
        [FieldOffset(0)]
        public int ia;
        [FieldOffset(4)]
        public int ib;

        public Segment(int a, int b)
        {
            ID = 0;
            if (a > b)
            {
                ia = b; ib = a;
            }
            else
            {
                ia = a; ib = b;
            }
        }

        public bool Equals(Segment other)
        {
            return ID == other.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public unsafe static Segment FromPointer(Point2D* begin, Point2D* p1, Point2D* p2)
        {
            return new((int)(p1 - begin), (int)(p2 - begin));
        }
    }
}
