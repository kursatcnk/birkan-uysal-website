using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Birkan.Web.Services;

// Panelin tek bir kullanıcısı var. Şifre düz metin olarak hiçbir yerde durmuyor; App_Data'daki dosyada yalnızca hash'i var.
// Dosya ilk açılışta BirkanPanel:Username ve BirkanPanel:InitialPassword ayarlarından oluşturuluyor; repoya girmiyor.
public sealed class BirkanAdminStore
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly string _path;
    private readonly PasswordHasher<string> _hasher = new();
    private readonly Lock _gate = new();
    private BirkanAdminAccount _account;
    private int _failures;
    private DateTimeOffset _lockedUntil = DateTimeOffset.MinValue;

    public BirkanAdminStore(IWebHostEnvironment env, IConfiguration config)
    {
        _path = Path.Combine(env.ContentRootPath, "App_Data", "birkan-admin.json");

        if (File.Exists(_path))
        {
            _account = JsonSerializer.Deserialize<BirkanAdminAccount>(File.ReadAllText(_path))
                ?? throw new InvalidOperationException("birkan-admin.json okunamadı.");
            return;
        }

        var username = config["BirkanPanel:Username"];
        var password = config["BirkanPanel:InitialPassword"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "Panel hesabı yok. İlk açılış için BirkanPanel:Username ve BirkanPanel:InitialPassword ayarlarını verin (ortam değişkeni ya da user-secrets).");

        _account = new BirkanAdminAccount(username, _hasher.HashPassword(username, password), DateTimeOffset.Now);
        Persist();
    }

    public string Username => _account.Username;
    public DateTimeOffset PasswordChangedAt => _account.PasswordChangedAt;

    public BirkanLoginResult Verify(string username, string password)
    {
        lock (_gate)
        {
            if (DateTimeOffset.Now < _lockedUntil)
                return BirkanLoginResult.LockedOut;

            var ok = string.Equals(username.Trim(), _account.Username, StringComparison.OrdinalIgnoreCase)
                && _hasher.VerifyHashedPassword(_account.Username, _account.PasswordHash, password) != PasswordVerificationResult.Failed;

            if (ok)
            {
                _failures = 0;
                return BirkanLoginResult.Success;
            }

            if (++_failures >= MaxFailures)
            {
                _failures = 0;
                _lockedUntil = DateTimeOffset.Now.Add(LockoutDuration);
                return BirkanLoginResult.LockedOut;
            }

            return BirkanLoginResult.Failed;
        }
    }

    public bool ChangePassword(string currentPassword, string newPassword)
    {
        lock (_gate)
        {
            if (_hasher.VerifyHashedPassword(_account.Username, _account.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
                return false;

            _account = _account with
            {
                PasswordHash = _hasher.HashPassword(_account.Username, newPassword),
                PasswordChangedAt = DateTimeOffset.Now
            };
            Persist();
            return true;
        }
    }

    private void Persist()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(_account, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed record BirkanAdminAccount(string Username, string PasswordHash, DateTimeOffset PasswordChangedAt);
}

public enum BirkanLoginResult
{
    Success,
    Failed,
    LockedOut
}
