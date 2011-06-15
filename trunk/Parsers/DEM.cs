using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PaperBag.Parsers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    class DemoHeaderReader
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
    }

    
    public class DemoHeader
    {
        private readonly DemoHeaderReader source;
        private DemoHeader(DemoHeaderReader source)
        {
            this.source = source;
        }

        public TimeSpan Length
        {
            get
            {
                return TimeSpan.FromSeconds(source.PlaybackTime);
            }
        }

        public string MapName
        {
            get
            {
                return source.MapName;
            }
        }

        public string PlayerName
        {
            get
            {
                return source.ClientName;
            }
        }

        public string Server
        {
            get
            {
                return source.ServerName;
            }
        }

        public static DemoHeader Read(string demoPath)
        {
            using (var demoStream = new FileStream(demoPath, FileMode.Open))
            using (var demoReader = new BinaryReader(demoStream))
                return demoReader.Read<DemoHeader>();
        }
    }
}