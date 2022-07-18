using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WebServerImages.Models.Images;
using System.Linq;

namespace WebServerImages.Services;

public class ImageService : IImageService
{
    private const int ThumbnailWidth = 600; // 300
    private const int FullScreenWidth = 1000;

    public async Task Process(IEnumerable<ImageInputModel> images)
    {
        var tasks = (images.Select(image => Task.Run(async () =>
        {
            try
            {
                using var imageResult = await Image.LoadAsync(image.Content);

                await this.SaveImage(imageResult, $"Original_{image.Name}", imageResult.Width);
                await this.SaveImage(imageResult, $"FullScreen_{image.Name}", FullScreenWidth);
                await this.SaveImage(imageResult, $"Thumbnail_{image.Name}", ThumbnailWidth);
            }
            catch
            {
                // Log Information.
            }
        }))).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task SaveImage(Image image, string name, int resizeWidth)
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

        //await using var memoryStream = new MemoryStream();
        //await imageResult.SaveAsJpeg(memoryStream, new JpegEncoder);

        await image.SaveAsJpegAsync(name, new JpegEncoder
        {
            Quality = 75
        });
    }
}
