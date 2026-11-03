using System.Security.Claims;
using Birkan.Web.Models;
using Birkan.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Birkan.Web.Panel;

[Authorize]
[AutoValidateAntiforgeryToken]
[Route("birkan-panel")]
public sealed class BirkanPanelController(
    BirkanContentStore store,
    BirkanAdminStore admin,
    BirkanMediaService media) : Controller
{
    private const string Saved = "Değişiklikler kaydedildi, site güncellendi.";

    // ---- Giriş / çıkış ----

    [AllowAnonymous]
    [HttpGet("giris")]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Index));
        return View(new BirkanLoginForm { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [EnableRateLimiting("birkan-login")]
    [HttpPost("giris")]
    public async Task<IActionResult> Login(BirkanLoginForm form)
    {
        var result = admin.Verify(form.Username ?? "", form.Password ?? "");
        if (result != BirkanLoginResult.Success)
        {
            ViewData["Error"] = result == BirkanLoginResult.LockedOut
                ? "Çok fazla hatalı deneme yapıldı. 15 dakika sonra tekrar deneyin."
                : "Kullanıcı adı ya da şifre hatalı.";
            form.Password = null;
            return View(form);
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, admin.Username)], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Url.IsLocalUrl(form.ReturnUrl) ? LocalRedirect(form.ReturnUrl!) : RedirectToAction(nameof(Index));
    }

    [HttpPost("cikis")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ---- Kontrol paneli ----

    [HttpGet("")]
    public IActionResult Index()
    {
        var backups = store.Backups();
        return View(new BirkanDashboard(store.Current, backups.FirstOrDefault()?.LastWriteTime, backups.Count));
    }

    // ---- Bölüm düzenleyicileri ----

    [HttpGet("genel")]
    public IActionResult General() => View(new BirkanGeneralForm { Site = store.Current.Site, Hero = store.Current.Hero });

    [HttpPost("genel")]
    public Task<IActionResult> General(BirkanGeneralForm form) => SaveAsync(nameof(General), c =>
    {
        c.Site = BirkanSafe.Normalize(form.Site);
        c.Hero = BirkanSafe.Normalize(form.Hero);
    });

    [HttpGet("hakkimda")]
    public IActionResult About() => View(store.Current.About);

    [HttpPost("hakkimda")]
    public Task<IActionResult> About(BirkanAbout form) => SaveAsync(nameof(About), c => c.About = BirkanSafe.Normalize(form));

    [HttpGet("egitim")]
    public IActionResult Education() => View(store.Current.Education);

    [HttpPost("egitim")]
    public Task<IActionResult> Education(BirkanSection<BirkanEducation> form) =>
        SaveAsync(nameof(Education), c => c.Education = BirkanSafe.Normalize(form));

    [HttpGet("deneyim")]
    public IActionResult Experience() => View(store.Current.Experience);

    [HttpPost("deneyim")]
    public Task<IActionResult> Experience(BirkanSection<BirkanExperience> form) =>
        SaveAsync(nameof(Experience), c => c.Experience = BirkanSafe.Normalize(form));

    [HttpGet("yetenekler")]
    public IActionResult Skills() => View(store.Current.Skills);

    [HttpPost("yetenekler")]
    public Task<IActionResult> Skills(BirkanSection<BirkanSkill> form) =>
        SaveAsync(nameof(Skills), c => c.Skills = BirkanSafe.Normalize(form));

    [HttpGet("projeler")]
    public IActionResult Projects() => View(store.Current.Projects);

    [HttpPost("projeler")]
    public Task<IActionResult> Projects(BirkanSection<BirkanProject> form) =>
        SaveAsync(nameof(Projects), c => c.Projects = BirkanSafe.Normalize(form));

    [HttpGet("iletisim")]
    public IActionResult Contact() => View(store.Current.Contact);

    [HttpPost("iletisim")]
    public Task<IActionResult> Contact(BirkanSection<BirkanContactItem> form) =>
        SaveAsync(nameof(Contact), c => c.Contact = BirkanSafe.Normalize(form));

    // ---- Görsel yükleme (formlar fetch ile çağırıyor) ----

    [HttpPost("yukle")]
    [RequestSizeLimit(BirkanMediaService.MaxUploadBytes + 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile? file, string kind)
    {
        if (file is null) return BadRequest(new { error = "Dosya seçilmedi." });
        try
        {
            return Json(new { path = await media.SaveAsync(file, kind) });
        }
        catch (BirkanMediaException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ---- Yedekler ----

    [HttpGet("yedekler")]
    public IActionResult Backups() => View(store.Backups());

    [HttpPost("yedekler/geri-yukle")]
    public async Task<IActionResult> Restore(string name)
    {
        TempData["Notice"] = await store.RestoreAsync(name)
            ? "Seçilen yedek geri yüklendi. Geri yüklemeden önceki hâl de yedeklere eklendi."
            : "Yedek bulunamadı.";
        return RedirectToAction(nameof(Backups));
    }

    // ---- Şifre ----

    [HttpGet("sifre")]
    public IActionResult Password() => View(new BirkanPasswordForm());

    [HttpPost("sifre")]
    public IActionResult Password(BirkanPasswordForm form)
    {
        if (string.IsNullOrEmpty(form.New) || form.New.Length < 8)
            ViewData["Error"] = "Yeni şifre en az 8 karakter olmalı.";
        else if (form.New != form.Repeat)
            ViewData["Error"] = "Yeni şifreler birbiriyle aynı değil.";
        else if (!admin.ChangePassword(form.Current ?? "", form.New))
            ViewData["Error"] = "Mevcut şifre hatalı.";
        else
        {
            TempData["Notice"] = "Şifre değiştirildi.";
            return RedirectToAction(nameof(Password));
        }
        return View(new BirkanPasswordForm());
    }

    private async Task<IActionResult> SaveAsync(string action, Action<BirkanContent> apply)
    {
        var content = store.Snapshot();
        apply(content);
        await store.SaveAsync(content);
        TempData["Notice"] = Saved;
        return RedirectToAction(action);
    }
}
