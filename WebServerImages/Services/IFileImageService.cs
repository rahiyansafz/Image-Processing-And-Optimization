using WebServerImages.Models.Images;

namespace WebServerImages.Services;

public interface IFileImageService
{
    Task Process(IEnumerable<ImageInputModel> images);
    Task<List<string>> GetAllImages();
}
