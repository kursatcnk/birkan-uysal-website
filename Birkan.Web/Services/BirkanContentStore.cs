using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Birkan.Web.Models;

namespace Birkan.Web.Services;

// İçerik tek bir JSON dosyasında tutuluyor; site her istekte bellekteki kopyayı okuyor, panel kaydettiğinde dosya ve kopya birlikte yenileniyor.
public sealed class BirkanContentStore
{
    private const int BackupLimit = 30;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        // Türkçe karakterler dosyada ğ gibi kaçış dizilerine dönmesin, elle de okunabilsin.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private readonly string _path;
    private readonly string _backupDir;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private BirkanContent _current;

    public BirkanContentStore(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        _path = Path.Combine(dataDir, "birkan-content.json");
        _backupDir = Path.Combine(dataDir, "birkan-backups");
        _current = Load();
    }

    public BirkanContent Current => _current;

    // Panel formları üzerinde değişiklik yapılabilsin diye bağımsız bir kopya veriyor.
    public BirkanContent Snapshot() =>
        JsonSerializer.Deserialize<BirkanContent>(JsonSerializer.Serialize(_current, JsonOptions), JsonOptions)!;

    public async Task SaveAsync(BirkanContent content)
    {
        await _writeLock.WaitAsync();
        try
        {
            Directory.CreateDirectory(_backupDir);
            if (File.Exists(_path))
            {
                File.Copy(_path, Path.Combine(_backupDir, $"birkan-content-{DateTime.Now:yyyyMMdd-HHmmss-fff}.json"));
                PruneBackups();
            }

            // Önce geçici dosyaya yazılıp sonra yer değiştiriliyor; yazma yarıda kesilirse site bozuk JSON okumasın.
            var temp = _path + ".tmp";
            await File.WriteAllTextAsync(temp, JsonSerializer.Serialize(content, JsonOptions));
            File.Move(temp, _path, overwrite: true);
            _current = content;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public IReadOnlyList<FileInfo> Backups() =>
        Directory.Exists(_backupDir)
            ? new DirectoryInfo(_backupDir).GetFiles("birkan-content-*.json").OrderByDescending(f => f.Name).ToList()
            : [];

    public async Task<bool> RestoreAsync(string backupName)
    {
        var file = Backups().FirstOrDefault(f => f.Name == backupName);
        if (file is null) return false;

        var content = JsonSerializer.Deserialize<BirkanContent>(await File.ReadAllTextAsync(file.FullName), JsonOptions);
        if (content is null) return false;

        await SaveAsync(content);
        return true;
    }

    private BirkanContent Load() =>
        JsonSerializer.Deserialize<BirkanContent>(File.ReadAllText(_path), JsonOptions)
        ?? throw new InvalidOperationException("birkan-content.json okunamadı.");

    private void PruneBackups()
    {
        foreach (var old in Backups().Skip(BackupLimit))
            old.Delete();
    }
}
