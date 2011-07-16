using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PaperBag
{
    public static class Extensions
    {
        public static T Read<T>(this BinaryReader reader, int? Size = null)
        {
            int structSize = Size ?? Marshal.SizeOf(typeof(T));
            byte[] readBytes = reader.ReadBytes(structSize);
            if (readBytes.Length != structSize)
                throw new ArgumentException("Size of bytes read did not match struct size");

            var pin_bytes = GCHandle.Alloc(readBytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(pin_bytes.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                pin_bytes.Free();
            }
        }
    }
}
