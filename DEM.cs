using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PaperBag
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DEM
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string Header;

        public int DemoProtocolVersion;
        public int NetworkProtocolVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string ServerName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string ClientName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string MapName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string GameDirectory;

        public float PlaybackTime;
        public int Ticks;
        public int Frames;
        public int SignOnLength;

        public static DEM ReadDemoHeader(string demoPath)
        {
            using (var demoStream = new FileStream(demoPath, FileMode.Open))
            using (var demoReader = new BinaryReader(demoStream))
                return demoReader.Read<DEM>();
        }
    }
}
