using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorTodo.Abstraction.Services;
using SkiaSharp;

namespace RazorTodo.Service
{
    public class ImageService : IImageService
    {
        public FileInfo ResizeImage(Stream s, string fileName, int maxDimension)
        {
            FileInfo fi = null;
            using (SKImage img = SKImage.FromEncodedData(s))
            {
                double aspect = img.Width / (img.Height * 1.0);
                Size dimension = new Size(img.Width, img.Height);
                if (aspect >= 1)
                {
                    if (img.Width > maxDimension)
                    {
                        int w = maxDimension;
                        int h = (int)Math.Floor(maxDimension / aspect);
                        dimension = new Size(w, h);
                    }
                }
                else
                {
                    if (img.Height > maxDimension)
                    {
                        int w = (int)Math.Floor(maxDimension * aspect);
                        int h = maxDimension;
                        dimension = new Size(w, h);
                    }
                }
                using (var surface = SKSurface.Create(new SKImageInfo(dimension.Width, dimension.Height)))
                {
                    surface.Canvas.DrawImage(img, new SKRect(0, 0, img.Width, img.Height), new SKRect(0, 0, dimension.Width, dimension.Height));
                    using (var resized = surface.Snapshot())
                    {
                        using (var data = resized.Encode(SKEncodedImageFormat.Webp, 90))
                        {
                            var tmpDir = Directory.CreateDirectory(Guid.NewGuid().ToString());
                            string newFileName = "resize.webp";
                            string[] fileNameParts = fileName.Split(".", StringSplitOptions.RemoveEmptyEntries);
                            if(fileNameParts.Length >= 2)
                            {
                                newFileName = $"{fileNameParts[0]}.webp";
                            }
                            else
                            {
                                newFileName = fileName + ".webp";
                            }
                            string tmpFileName = Path.Combine(tmpDir.Name, newFileName);
                            using (var fs = new FileStream(tmpFileName, FileMode.Create, FileAccess.Write))
                            {
                                data.SaveTo(fs);
                            }
                            fi = new FileInfo(tmpFileName);
                        }
                    }
                }
            }
            return fi;
        }
    }
}
