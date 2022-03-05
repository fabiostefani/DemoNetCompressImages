
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
        private const string ExtensionPng = "png";

        public CompressImagesService()
        {
            this.quality = 20;
        }
        
        public async Task CompressImage(IFormFile image)
        {            
            await File.WriteAllBytesAsync(MakePath($"original_{image.Name}", "jpg"), await ReadAllBytesFile(image));            

            await SharpCompactJPG(image, MakePath($"ImageSharp_{image.Name}", "jpg"));                
            await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}", ExtensionPng), PngCompressionLevel.BestCompression);
            await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}2", ExtensionPng), PngCompressionLevel.DefaultCompression);
            await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L0", ExtensionPng), PngCompressionLevel.Level0);
            await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L1", ExtensionPng), PngCompressionLevel.Level1);
            await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L2", ExtensionPng), PngCompressionLevel.Level2);
            await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L5", ExtensionPng), PngCompressionLevel.Level5);
            CompresswebP(image);
        }

        private string MakePath(string fileName, string extension)
        {
            return Path.Combine("Images", $"{fileName}.{extension}");
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
                                .Quality(90) //parametro para não perder a qualidade no momento da compressão
                                .Save(webPFileStream);

                }
            }

        }

        public async Task SharpCompactJPG(IFormFile image, string path)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.CopyTo(memoryStream);
                memoryStream.Position = 0L;                
                using var ms = new FileStream(path, FileMode.CreateNew);

                var imageSharp = SixLabors.ImageSharp.Image.Load(memoryStream);
                imageSharp.Metadata.ExifProfile = null;
                await imageSharp.SaveAsJpegAsync(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder()
                {
                    Quality = (int)quality,
                    Subsample = SixLabors.ImageSharp.Formats.Jpeg.JpegSubsample.Ratio444                
                });
            }   
        }

        public async Task SharpCompactPNG(IFormFile image, string path, PngCompressionLevel level)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.CopyTo(memoryStream);
                memoryStream.Position = 0L;                
                using var ms = new FileStream(path, FileMode.CreateNew);

                var imageSharp = SixLabors.ImageSharp.Image.Load(memoryStream);
                imageSharp.Metadata.ExifProfile = null;            

                await imageSharp.SaveAsPngAsync(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder()
                {
                    CompressionLevel = level,
                    ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.Palette,                
                });
            }           
        }

        
    }
}