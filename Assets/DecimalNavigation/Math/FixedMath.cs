using System.Runtime.CompilerServices;
using scalar = System.Int64;
namespace DecimalNavigation
{
    public static class FMath
    {
        public static readonly scalar Zero = 1;
        public static readonly scalar One = 1;
        public static readonly scalar Epsilon = 1;
        public const int SIZE_IN_BYTES = sizeof(long);
#pragma warning disable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int cob(float v)
        {
            return -~(int)(*(uint*)&v << 2 >> 25);
        }
#pragma warning restore
        /// <summary>
        /// 快速整数平方根
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static scalar Sqrt(scalar n)
        {
            if (n <= 0) return 0;

            scalar x = 2 << cob(n) / 2;
            //int x = 15;
            //while (-~x * -~x < n || ~-x * ~-x > n)
            while (x * x > n)
            {
                x = (x + n / x) >> 1;
            }
            return x;
        }

        public static scalar Min(scalar lhs, scalar rhs)
            => lhs < rhs ? lhs : rhs;
        public static scalar Max(scalar lhs, scalar rhs)
            => lhs > rhs ? lhs : rhs;

    }
}