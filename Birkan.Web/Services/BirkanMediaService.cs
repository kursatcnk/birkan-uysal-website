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

        // Tür dosya adına giriyor; yalnızca bilinen değerler kabul ediliyor.
        if (!MaxWidths.TryGetValue(kind, out var maxWidth))
            throw new BirkanMediaException("Geçersiz görsel türü.");

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

    // Özgeçmiş gibi PDF belgeler: gerçekten PDF mi diye başlığına bakılıyor; içinde betik ya da
    // açılışta çalışan eylem barındıran dosyalar reddediliyor, çünkü site ziyaretçilerine dağıtılıyor.
    public async Task<string> SaveDocumentAsync(IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxUploadBytes)
            throw new BirkanMediaException("Dosya boş ya da 15 MB'tan büyük.");

        byte[] bytes;
        await using (var input = file.OpenReadStream())
        using (var buffer = new MemoryStream())
        {
            await input.CopyToAsync(buffer);
            bytes = buffer.ToArray();
        }

        if (bytes.Length < 5 || System.Text.Encoding.ASCII.GetString(bytes, 0, 5) != "%PDF-")
            throw new BirkanMediaException("Yalnızca PDF dosyası yüklenebilir.");

        var text = System.Text.Encoding.Latin1.GetString(bytes);
        foreach (var marker in new[] { "/JavaScript", "/JS", "/Launch", "/EmbeddedFile", "/OpenAction", "/RichMedia" })
        {
            if (text.Contains(marker, StringComparison.Ordinal))
                throw new BirkanMediaException("PDF içinde betik ya da otomatik çalışan eylem var; bu dosya yüklenemez.");
        }

        Directory.CreateDirectory(_uploadDir);
        var name = $"birkan-doc-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..6]}.pdf";
        await File.WriteAllBytesAsync(Path.Combine(_uploadDir, name), bytes);

        return $"/birkan-media/uploads/{name}";
    }
}

public sealed class BirkanMediaException(string message) : Exception(message);
