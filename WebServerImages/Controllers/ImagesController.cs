using Microsoft.AspNetCore.Mvc;
using WebServerImages.Models.Images;
using WebServerImages.Services;

namespace WebServerImages.Controllers;

public class ImagesController : Controller
{
    private readonly IImageService imageService;

    public ImagesController(IImageService imageService)
        => this.imageService = imageService;

    [HttpGet]
    public IActionResult Index() => this.View();

    [HttpPost]
    [RequestSizeLimit(100 * 1024 * 1024)] // (24 * 1024 * 1024)
    public async Task<IActionResult> Index(IFormFile[] images)
    {
        if (images.Length > 20)
        {
            this.ModelState.AddModelError("images", "you cannot upload more than 5 images!");
            return this.View();
        }

        await this.imageService.Process(images.Select(i => new ImageInputModel
        {
            Name = i.FileName,
            Type = i.ContentType,
            Content = i.OpenReadStream()
        }));

        return this.RedirectToAction(nameof(this.Done));
    }

    public IActionResult Done() => this.View();
}
