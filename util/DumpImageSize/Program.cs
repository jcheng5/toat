using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace DumpImageSize
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                string exeName = Path.GetFileName(Process.GetCurrentProcess().Modules[0].FileName);
                Console.Error.WriteLine("Usage: " + exeName + " <Directory> <OutputFile>");
                return 1;
            }

            string dir = Path.Combine(Directory.GetCurrentDirectory(), args[0]);
            string outfile = args[1];

            using (var output = new StreamWriter(outfile, false, Encoding.ASCII))
                WriteFileSizes(dir, output);
            return 0;
        }

        static void WriteFileSizes(string dir, TextWriter output)
        {
            // Read all the image files from the specified directory
            foreach (var file in Directory.GetFiles(dir))
            {
                switch (Path.GetExtension(file).ToLowerInvariant())
                {
                    case ".png":
                    case ".gif":
                    case ".jpg":
                        break;
                    default:
                        continue;
                }
                string absFile = Path.Combine(dir, file);
                using (Bitmap bitmap = new Bitmap(absFile))
                {
                    int width = bitmap.Width;
                    int height = bitmap.Height;
                    output.WriteLine("{0}\t{1}\t{2}", Path.GetFileName(file), width, height);
                }
            }
        }
    }
}
