namespace WebServerImages.Models.Images;

public class ImageInputModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Stream Content { get; set; }
}
