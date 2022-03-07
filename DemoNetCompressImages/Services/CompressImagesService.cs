using System.Diagnostics;
using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
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
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();            
            await SharpCompactJPG(image, MakePath($"ImageSharp_{image.Name}", "jpg"));                
            stopWatch.Stop();            
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime JPG " + elapsedTime);

            // await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}", ExtensionPng), PngCompressionLevel.BestCompression);
            // await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}2", ExtensionPng), PngCompressionLevel.DefaultCompression);
            // await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L0", ExtensionPng), PngCompressionLevel.Level0);
            // await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L1", ExtensionPng), PngCompressionLevel.Level1);
            // await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L2", ExtensionPng), PngCompressionLevel.Level2);
            // await SharpCompactPNG(image, MakePath($"ImageSharp_{image.Name}L5", ExtensionPng), PngCompressionLevel.Level5);
            stopWatch.Start();            
            CompresswebP(image);
            stopWatch.Stop();            
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime WEBP " + elapsedTime);
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
            using (var webPFileStream = new FileStream(Path.Combine("Images", $"webp_{image.Name}.webp"), FileMode.Create))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(image.OpenReadStream()) //carregando os dados da imagem
                                .Format(new WebPFormat()) //formato
                                .Quality(100)                                 
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