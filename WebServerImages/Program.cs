using Microsoft.EntityFrameworkCore;
using WebServerImages.Data;
using WebServerImages.Services;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IFileImageService, FileImageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = options =>
    {

        var headers = options.Context.Response.GetTypedHeaders();

        headers.CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(30)
        };

        headers.Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(30));
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
