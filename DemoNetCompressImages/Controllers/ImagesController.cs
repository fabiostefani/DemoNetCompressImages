using DemoNetCompressImages.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoNetCompressImages.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> logger;
        private readonly ICompressImagesService compressImagesService;
        public ImagesController(ILogger<ImagesController> logger,
                                ICompressImagesService compressImagesService)
        {
            this.compressImagesService = compressImagesService;
            this.logger = logger;
        }    

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile formFile)
        {
            if (formFile == null) return BadRequest("Inválid file");
            await this.compressImagesService.CompressImage(formFile);
            return Ok();
        }
    }
}