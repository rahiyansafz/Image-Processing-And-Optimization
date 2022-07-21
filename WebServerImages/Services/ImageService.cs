using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WebServerImages.Models.Images;
using System.Linq;
using WebServerImages.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace WebServerImages.Services;

public class ImageService : IImageService
{
    private const int ThumbnailWidth = 600; // 300
    private const int FullScreenWidth = 1000;

    private readonly ApplicationDbContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ImageService(ApplicationDbContext context, IServiceScopeFactory serviceScopeFactory)
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task<List<string>> GetAllImages() => _context.ImageData.Select(i => i.Id.ToString()).ToListAsync();

    public Task<Stream> GetFullScreen(string id) => GetImageData(id, "FullScreen");

    public Task<Stream> GetThumbnail(string id) => GetImageData(id, "Thumbnail");

    public async Task Process(IEnumerable<ImageInputModel> images)
    {
        var tasks = (images.Select(image => Task.Run(async () =>
        {
            try
            {
                using var imageResult = await Image.LoadAsync(image.Content);

                var original = await SaveImage(imageResult, imageResult.Width);
                var fullscreen = await SaveImage(imageResult, FullScreenWidth);
                var thumbnail = await SaveImage(imageResult, ThumbnailWidth);

                var database = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

                database.ImageData.Add(new ImageData
                {
                    OriginalFileName = image.Name,
                    OriginalType = image.Type,
                    OriginalContent = original,
                    ThumbnailContent = thumbnail,
                    FulLScreenContent = fullscreen,
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

    private async Task<Stream> GetImageData(string id, string size)
    {
        var database = _context.Database;

        var dbConnection = (SqlConnection)database.GetDbConnection();

        var command = new SqlCommand(
            $"SELECT {size}Content FROM ImageData WHERE Id = @id;", dbConnection);

        command.Parameters.Add(new SqlParameter("@id", id));

        dbConnection.Open();

        var reader = await command.ExecuteReaderAsync();

        Stream result = null;

        if (reader.HasRows)
            while (reader.Read()) result = reader.GetStream(0);

        reader.Close();

        return result;
    }
}
