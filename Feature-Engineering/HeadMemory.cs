using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Feature_Engineering
{
    public class FastHeadMemory
    {
        private const int Length = 12288;   // 3x64x64

        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        public static MemoryPin Rent(string path)
        {
            byte[] buffer = Pool.Rent(Length);
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, FileOptions.SequentialScan);
                int read = fs.Read(buffer, 0, Length);
                if (read < Length)           
                    Array.Clear(buffer, read, Length - read);

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                return new MemoryPin(handle, buffer, Length);
            }
            catch
            {
                Pool.Return(buffer);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(in MemoryPin pin)
        {
            if (pin.Handle.IsAllocated)
                pin.Handle.Free();
            if (pin.Array != null)
                Pool.Return(pin.Array);
        }
    }
}