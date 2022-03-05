
using System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace DemoNetCompressImages.Services
{
    public class CompressImagesService : ICompressImagesService
    {
        public long quality { get; set; }

        public CompressImagesService()
        {
            this.quality = 20;
        }
        
        public async Task CompressImage(IFormFile image)
        {
            byte[] file = await ReadAllBytesFile(image);            
            string path = Path.Combine("Images", $"original_{image.FileName}");
            await File.WriteAllBytesAsync(path, file);
            // CompresswebP(image);

            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                ms.Position = 0L;
                SharpCompactJPG(ms, Path.Combine("Images", $"ImageSharp_{image.Name}.jpg"));                
            }
            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                ms.Position = 0L;
                SharpCompactPNG(ms, Path.Combine("Images", $"ImageSharp_{image.Name}.png"), PngCompressionLevel.BestCompression);
            }            

            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                ms.Position = 0L;
                SharpCompactPNG(ms, Path.Combine("Images", $"ImageSharp_{image.Name}2.png"), PngCompressionLevel.DefaultCompression);
            }   

            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                ms.Position = 0L;
                CoreCompact(ms, Path.Combine("Images", $"CoreCompact_{image.Name}2.png"));
            }   
            
        }

        private async Task<byte[]> ReadAllBytesFile(IFormFile image)
        {
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private void CompresswebP(IFormFile image)
        {
            using (var stream = new FileStream(Path.Combine("Images", image.FileName), FileMode.Create))
            {
                image.CopyTo(stream);
            }

            using (var webPFileStream = new FileStream(Path.Combine("Images", $"webp_{image.Name}.webp"), FileMode.Create))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(image.OpenReadStream()) //carregando os dados da imagem
                                .Format(new WebPFormat()) //formato
                                .Quality(100) //parametro para não perder a qualidade no momento da compressão
                                .Save(webPFileStream); //salvando a imagem
                }
            }

        }

        private void CompressMagickNet(IFormFile image)
        {
            // var snakewareLogo = new FileInfo(Path.Combine("Images", image.FileName));

            // Console.WriteLine("Bytes before: " + snakewareLogo.Length);

            // var optimizer = new ImageOptimizer();
            // optimizer.LosslessCompress(snakewareLogo);

            // snakewareLogo.Refresh();
            // Console.WriteLine("Bytes after:  " + snakewareLogo.Length);

            // using (MagickImage image = new MagickImage("input.svg"))
            // {
            // image.Scale(new Percentage(60));
            // image.Write("output.png");
            // }
        }

        public void SharpCompactJPG(Stream stream, string path)
        {
            // var filename = Path.Combine(path, "sharper.jpg");
            // if (File.Exists(filename))
            //     File.Delete(filename);

            using var ms = new FileStream(path, FileMode.CreateNew);

            var image = SixLabors.ImageSharp.Image.Load(stream);
            image.Metadata.ExifProfile = null;
            image.SaveAsJpeg(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder()
            {
                Quality = (int)quality,
                Subsample = SixLabors.ImageSharp.Formats.Jpeg.JpegSubsample.Ratio420
            });
        }

        public void SharpCompactPNG(Stream stream, string path, PngCompressionLevel level)
        {
            // var filename = Path.Combine(path, "sharper.png");
            // if (File.Exists(filename))
            //     File.Delete(filename);

            using var ms = new FileStream(path, FileMode.CreateNew);

            var image = SixLabors.ImageSharp.Image.Load(stream);
            image.Metadata.ExifProfile = null;

            image.SaveAsPng(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder()
            {
                CompressionLevel = level,
            ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.Palette,
            });
        }

        public void CoreCompact(Stream stream, string path)
        {
            // var filename = Path.Combine(path, "coredrawing.jpg");
            // if (File.Exists(filename))
            //     File.Delete(filename);


            using var image = new Bitmap(System.Drawing.Image.FromStream(stream));
            using var graphics = Graphics.FromImage(image);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(image, 0, 0);
            graphics.Flush(FlushIntention.Sync);

            using var ms = new FileStream(filename, FileMode.CreateNew);
            var qualityParamId = Encoder.Quality;
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
            var codec = ImageCodecInfo.GetImageDecoders()
                .FirstOrDefault(codecx => codecx.FormatID == ImageFormat.Jpeg.Guid);
            image.Save(ms, codec, encoderParameters);
        }
    }
}