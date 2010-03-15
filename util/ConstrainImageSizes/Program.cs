using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace ConstrainImageSizes
{
    class Program
    {
        const int ARG_MAXWIDTH = 0;
        const int ARG_MAXHEIGHT = 1;
        const int ARG_INDIR = 2;
        const int ARG_OUTDIR = 3;

        static int maxWidth;
        static int maxHeight;

        static int Main(string[] args)
        {
            if (args.Length < 4 
                || !int.TryParse(args[ARG_MAXWIDTH], out maxWidth) 
                || !int.TryParse(args[ARG_MAXHEIGHT], out maxHeight))
            {
                string exe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                Console.Error.WriteLine("Usage: " + exe + " <max_width> <max_height> <in_dir> <out_dir>");
                return 1;
            }

            if (maxWidth <= 0 || maxHeight <= 0)
            {
                Console.Error.WriteLine("Illegal value for max width or height");
                return 1;
            }

            string dir = args[ARG_INDIR];
            if (!Directory.Exists(dir))
            {
                Console.Error.WriteLine("Input directory does not exist");
                return 1;
            }

            string outdir = args[ARG_OUTDIR];
            if (!Directory.Exists(outdir))
            {
                Console.Error.WriteLine("Output directory does not exist");
                return 1;
            }

            foreach (var file in Directory.GetFiles(dir))
            {
                switch (Path.GetExtension(file).ToLowerInvariant())
                {
                    case ".jpg":
                        Resize(file, outdir, true);
                        break;
                    case ".png":
                    case ".gif":
                    case ".tif":
                    case ".bmp":
                        Resize(file, outdir, false);
                        break;
                }
            }

            return 0;
        }

        private static void Resize(string file, string outdir, bool useJpegCompression)
        {
            String dest = Path.Combine(outdir, Path.GetFileNameWithoutExtension(file) + (useJpegCompression ? ".jpg" : ".png"));

            using (Bitmap sourceImage = new Bitmap(file))
            {
                Size newSize = Constrain(sourceImage.Size);
                using (Bitmap newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(newImage))
                    {
                        g.CompositingMode = CompositingMode.SourceOver;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        g.DrawImage(sourceImage, 0, 0, newSize.Width, newSize.Height);
                    }

                    if (useJpegCompression)
                        newImage.Save(dest, ImageFormat.Jpeg);
                    else
                        newImage.Save(dest, ImageFormat.Png);

                    Console.WriteLine("{0} - {1}x{2}", dest, newSize.Width, newSize.Height);
                }
            }
        }

        private static Size Constrain(Size size)
        {
            double widthRatio = (double)size.Width / maxWidth;
            double heightRatio = (double)size.Height / maxHeight;

            if (widthRatio < 1 && heightRatio < 1)
                return size;

            double ratio = Math.Max(widthRatio, heightRatio);

            return new Size(
                (int)Math.Round(size.Width / ratio),
                (int)Math.Round(size.Height / ratio));
        }
    }
}
