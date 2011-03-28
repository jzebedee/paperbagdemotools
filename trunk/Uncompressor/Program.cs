using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Uncompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            //                FileCompression.Decompress(file, path + Path.GetFileNameWithoutExtension(file) + ".dem");*/
            foreach (var arg in args)
            {
                if (!File.Exists(arg))
                {
                    Console.WriteLine("{0} does not exist. Skipping.", arg);
                    continue;
                }

                var newPath = Path.ChangeExtension(arg, "dem");
                FileCompression.Decompress(arg, newPath);

                Console.WriteLine("Decompressed {0}, saved to {1}", Path.GetFileNameWithoutExtension(arg), newPath);
            }

            Console.WriteLine();
            Console.WriteLine("Decompression completed.");
            Console.ReadKey();
        }
    }
}