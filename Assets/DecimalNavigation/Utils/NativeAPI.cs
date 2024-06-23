using System.Runtime.InteropServices;

namespace DecimalNavigation
{

    public unsafe static class NativeAPI
    {
        public static void* MemAlloc(int bytes)
        {
            return (void*)Marshal.AllocHGlobal(bytes);
        }

        public static void MemFree(void* ptr)
        {
            Marshal.FreeHGlobal((System.IntPtr)ptr);
        }

        public static void MemSet(void* ptr, int len, byte value)
        {
            for (int i = 0; i < len; i++)
            {
                *((byte*)ptr + i) = value;
            }
        }

        public static void Memzero(void* ptr, int len)
        {
            for (int i = 0; i < len; i++)
            {
                *((byte*)ptr + i) = 0;
            }
        }
    }
}