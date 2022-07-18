namespace WebServerImages.Data;

public class ImageData
{
    public ImageData() => this.Id = Guid.NewGuid();
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; }
    public string OriginalType { get; set; }
    public byte[] OriginalContent { get; set; }
    public byte[] FulLScreenContent { get; set; }
    public byte[] ThumbnailContent { get; set; }

    //public string UserId { get; set; }
    //public int PostId { get; set; }
    //public int ArticleId { get; set; }
}
