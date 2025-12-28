using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Feature_Engineering
{
    public class FastHead
    {
        private const int Length = 12288;   // 3x64x64

        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        public static HeadMemoryPin Rent(string path)
        {
            byte[] buffer = Pool.Rent(Length);
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, FileOptions.SequentialScan);
                int read = fs.Read(buffer, 0, Length);
                if (read < Length)           
                    Array.Clear(buffer, read, Length - read);

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                return new HeadMemoryPin(handle, buffer, Length);
            }
            catch
            {
                Pool.Return(buffer);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(in HeadMemoryPin pin)
        {
            if (pin.Handle.IsAllocated)
                pin.Handle.Free();
            if (pin.Array != null)
                Pool.Return(pin.Array);
        }
    }

    public readonly struct HeadMemoryPin
    {
        public readonly IntPtr Ptr;    
        public readonly GCHandle Handle;
        public readonly byte[] Array;
        public readonly int Length;

        internal HeadMemoryPin(GCHandle handle, byte[] array, int length)
        {
            Handle = handle;
            Array = array;
            Length = length;
            Ptr = handle.AddrOfPinnedObject();
        }
    }
}