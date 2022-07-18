using Microsoft.EntityFrameworkCore;

namespace WebServerImages.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<ImageData> ImageData { get; set; }
}
