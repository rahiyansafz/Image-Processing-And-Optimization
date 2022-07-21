using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WebServerImages.Models.Images;
using WebServerImages.Services;

namespace WebServerImages.Controllers;

public class ImagesController : Controller
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
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

    public async Task<IActionResult> Thumbnail(string id) => ReturnImage(await _imageService.GetThumbnail(id));

    public async Task<IActionResult> FullScreen(string id) => ReturnImage(await _imageService.GetFullScreen(id));

    private IActionResult ReturnImage(Stream image)
    {
        var headers = Response.GetTypedHeaders();

        headers.CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(30)
        };

        headers.Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(30));

        return File(image, "image/jpeg");
    }

    public IActionResult Done() => View();
}
