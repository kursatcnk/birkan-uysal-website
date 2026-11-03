using Birkan.Web.Models;

namespace Birkan.Web.Panel;

public sealed class BirkanLoginForm
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class BirkanGeneralForm
{
    public BirkanSiteInfo Site { get; set; } = new();
    public BirkanHero Hero { get; set; } = new();
}

public sealed class BirkanPasswordForm
{
    public string? Current { get; set; }
    public string? New { get; set; }
    public string? Repeat { get; set; }
}

public sealed record BirkanDashboard(BirkanContent Content, DateTime? LastSaved, int BackupCount);
