namespace DemoNetCompressImages.Services
{
    public interface ICompressImagesService
    {
        Task CompressImage(IFormFile formFile);
    }
}