using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Feature_Engineering
{
    public readonly struct MemoryPin
    {
        public readonly IntPtr Ptr;
        public readonly GCHandle Handle;
        public readonly byte[] Array;
        public readonly int Length;

        internal MemoryPin(GCHandle handle, byte[] array, int length)
        {
            Handle = handle;
            Array = array;
            Length = length;
            Ptr = handle.AddrOfPinnedObject();
        }
    }
}
