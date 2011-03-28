using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SevenZip;

namespace Uncompressor
{
    public static class FileCompression
    {
        public static void Compress(string outPath, params string[] inPaths)
        {
            var Compressor = new SevenZipCompressor()
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionLevel = CompressionLevel.Fast,
                CompressionMethod = CompressionMethod.Lzma2,
                CompressionMode = CompressionMode.Create,
                FastCompression = true,
            };

            Compressor.CompressFiles(outPath, inPaths);
        }

        public static void Decompress(string inPath, string outPath)
        {
            using (var Extractor = new SevenZipExtractor(inPath))
            {
                using (var outStream = File.OpenWrite(outPath))
                {
                    Extractor.ExtractFile(0, outStream);

                    outStream.Flush();
                    outStream.Close();
                }
            }
        }
    }
}