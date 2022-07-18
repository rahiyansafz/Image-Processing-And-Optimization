using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WebServerImages.Models.Images;
using System.Linq;
using WebServerImages.Data;

namespace WebServerImages.Services;

public class ImageService : IImageService
{
    private const int ThumbnailWidth = 600; // 300
    private const int FullScreenWidth = 1000;

    private readonly IServiceScopeFactory serviceScopeFactory;

    public ImageService(IServiceScopeFactory serviceScopeFactory) => this.serviceScopeFactory = serviceScopeFactory;

    public async Task Process(IEnumerable<ImageInputModel> images)
    {
        var tasks = (images.Select(image => Task.Run(async () =>
        {
            try
            {
                using var imageResult = await Image.LoadAsync(image.Content);

                var original = await this.SaveImage(imageResult, imageResult.Width);
                var fullscreen = await this.SaveImage(imageResult, FullScreenWidth);
                var thumbnail = await this.SaveImage(imageResult, ThumbnailWidth);
            }
            catch
            {
            // Log Information.
        }
        }))).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task<byte[]> SaveImage(Image image, int resizeWidth)
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

        await using var memoryStream = new MemoryStream();

        await image.SaveAsJpegAsync(memoryStream, new JpegEncoder
        {
            Quality = 75
        });

        return memoryStream.ToArray();
    }
}
