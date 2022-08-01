using Microsoft.AspNetCore.Mvc;
using WebServerImages.Models.Images;
using WebServerImages.Services;

namespace WebServerImages.Controllers;

public class FileImagesController : Controller
{
    private readonly IFileImageService _imageService;

    public FileImagesController(IFileImageService imageService)
        => _imageService = imageService;

    [HttpGet]
    public IActionResult Index() => this.View();

    [HttpPost]
    [RequestSizeLimit(100 * 1024 * 1024)] // (24 * 1024 * 1024)
    public async Task<IActionResult> Index(IFormFile[] images)
    {
        if (images.Length > 20)
        {
            ModelState.AddModelError("images", "you cannot upload more than 5 images!");
            return this.View();
        }

        await _imageService.Process(images.Select(i => new ImageInputModel
        {
            Name = i.FileName,
            Type = i.ContentType,
            Content = i.OpenReadStream()
        }));

        return this.RedirectToAction(nameof(this.Done));
    }

    public async Task<IActionResult> All() => View(await _imageService.GetAllImages());

    public IActionResult Done() => View();
}

