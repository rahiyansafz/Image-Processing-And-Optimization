using WebServerImages.Models.Images;

namespace WebServerImages.Services;

public interface IImageService
{
    public Task Process(IEnumerable<ImageInputModel> images);
}
