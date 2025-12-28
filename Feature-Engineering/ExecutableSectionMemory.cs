using PeNet;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Feature_Engineering
{
    public static class FastExecutableSectionMemory
    {
        private const int Length = 12288;   // 3x64x64
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        public static MemoryPin Rent(string path)
        {
            byte[] buffer = Pool.Rent(Length);
            try
            {
                Unsafe.InitBlockUnaligned(ref buffer[0], 0, (uint)Length);

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                                              bufferSize: 1, FileOptions.SequentialScan);
                var pe = new PeFile(path);

                int dst = 0;
                if (pe.ImageSectionHeaders != null)
                {
                    foreach (var sec in pe.ImageSectionHeaders)
                    {
                        if (((uint)sec.Characteristics & 0x20000000) == 0) continue;

                        int off = (int)sec.PointerToRawData;
                        int len = (int)sec.SizeOfRawData;
                        if (off < 0 || len <= 0) continue;

                        int copy = (int)Math.Min(len, Length - dst);
                        if (copy <= 0) break;

                        fs.Position = off;
                        int got = fs.Read(new Span<byte>(buffer, dst, copy));
                        dst += got;
                        if (dst >= Length) break;
                    }
                }

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
            if (pin.Handle.IsAllocated) pin.Handle.Free();
            if (pin.Array != null) Pool.Return(pin.Array);
        }
    }
}