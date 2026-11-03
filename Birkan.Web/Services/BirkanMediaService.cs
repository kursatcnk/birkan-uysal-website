using SkiaSharp;

namespace Birkan.Web.Services;

// Panelden yüklenen her görsel yeniden çizilip WebP olarak kaydediliyor.
// Böylece dosya gerçekten görsel mi diye doğrulanmış oluyor, EXIF/konum bilgisi siliniyor ve boyut makul kalıyor.
public sealed class BirkanMediaService(IWebHostEnvironment env)
{
    public const long MaxUploadBytes = 15 * 1024 * 1024;

    private static readonly Dictionary<string, int> MaxWidths = new()
    {
        ["logo"] = 800,
        ["photo"] = 2400
    };

    private readonly string _uploadDir = Path.Combine(env.WebRootPath, "birkan-media", "uploads");

    public async Task<string> SaveAsync(IFormFile file, string kind)
    {
        if (file.Length == 0 || file.Length > MaxUploadBytes)
            throw new BirkanMediaException("Dosya boş ya da 15 MB'tan büyük.");

        var maxWidth = MaxWidths.GetValueOrDefault(kind, 2400);

        await using var input = file.OpenReadStream();
        using var source = SKBitmap.Decode(input)
            ?? throw new BirkanMediaException("Dosya okunamadı. JPG, PNG ya da WebP yükleyin.");

        var scale = Math.Min(1f, maxWidth / (float)source.Width);
        var info = new SKImageInfo((int)Math.Round(source.Width * scale), (int)Math.Round(source.Height * scale), SKColorType.Rgba8888, SKAlphaType.Premul);

        using var resized = source.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell))
            ?? throw new BirkanMediaException("Görsel yeniden boyutlandırılamadı.");
        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Webp, kind == "photo" ? 86 : 92);

        Directory.CreateDirectory(_uploadDir);
        var name = $"birkan-{kind}-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..6]}.webp";
        await using (var output = File.Create(Path.Combine(_uploadDir, name)))
            data.SaveTo(output);

        return $"/birkan-media/uploads/{name}";
    }
}

public sealed class BirkanMediaException(string message) : Exception(message);
