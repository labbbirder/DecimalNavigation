namespace com.bbbirder.Collections
{
    internal static class CollectionUtility
    {
        public unsafe static int CeilExponent(int n)
        {
            float f = n;
            var exp = (int)(*(uint*)&f << 1 >> 24) - 127;
            // var exp = (*(int*)&f << 1 >>> 24) - 127; // C# 11.0
            if (1 << exp < n) exp++;
            return exp;
        }
    }
}