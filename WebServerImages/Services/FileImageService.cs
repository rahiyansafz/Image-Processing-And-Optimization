using WebServerImages.Models.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WebServerImages.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace WebServerImages.Services;

public class FileImageService : IFileImageService
{
    private const int ThumbnailWidth = 600; // 300
    private const int FullScreenWidth = 1000;

    private readonly ApplicationDbContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FileImageService(ApplicationDbContext context, IServiceScopeFactory serviceScopeFactory)
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task<List<string>> GetAllImages() => _context.ImageFiles.Select(i => i.Folder + "/Thumbnail_" + i.Id + ".jpg").ToListAsync();

    public async Task Process(IEnumerable<ImageInputModel> images)
    {
        var totalImages = await _context.ImageFiles.CountAsync();

        var tasks = (images.Select(image => Task.Run(async () =>
        {
            try
            {
                using var imageResult = await Image.LoadAsync(image.Content);

                var id = Guid.NewGuid();
                var path = $"/images/{totalImages % 1000}/";
                var name = $"{id}.jpg";

                var storagePath = Path.Combine(
                Directory.GetCurrentDirectory(), $"wwwroot{path}".Replace("/", "\\"));

                if (!Directory.Exists(storagePath))
                    Directory.CreateDirectory(storagePath);

                await SaveImage(imageResult, $"Original_{name}", storagePath, imageResult.Width);
                await SaveImage(imageResult, $"Fullscreen_{name}", storagePath, FullScreenWidth);
                await SaveImage(imageResult, $"Thumbnail_{name}", storagePath, ThumbnailWidth);

                var database = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

                database.ImageFiles.Add(new ImageFile
                {
                    Id = id,
                    Folder = path
                });

                await database.SaveChangesAsync();
            }
            catch
            {
                // Log Information.
            }
        }))).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task SaveImage(Image image, string name, string path, int resizeWidth)
    {
        var width = image.Width;
        var height = image.Height;

        if (width > resizeWidth)
        {
            height = (int)((double)resizeWidth / width * height);
            width = resizeWidth;
        }

        image.Mutate(i => i.Resize(new Size(width, height)));

        image.Metadata.ExifProfile = null;

        await image.SaveAsJpegAsync($"{path}/{name}", new JpegEncoder
        {
            Quality = 75
        });
    }
}
