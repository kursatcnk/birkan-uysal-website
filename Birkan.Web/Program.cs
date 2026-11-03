using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using Birkan.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");

// Uygulama yeniden başladığında panel oturumu düşmesin diye anahtarlar diskte tutuluyor.
builder.Services.AddDataProtection()
    .SetApplicationName("Birkan")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDir, "birkan-keys")));

builder.Services.AddSingleton<BirkanContentStore>();
builder.Services.AddSingleton<BirkanAdminStore>();
builder.Services.AddSingleton<BirkanMediaService>();

// Razor çıktısında Türkçe karakterler &#x131; gibi kodlanmasın.
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/birkan-panel/giris";
        o.LogoutPath = "/birkan-panel/cikis";
        o.AccessDeniedPath = "/birkan-panel/giris";
        o.Cookie.Name = "birkan.panel";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// Giriş formuna kaba kuvvet denemelerini yavaşlatmak için IP başına dakikada 8 deneme.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("birkan-login", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 8, Window = TimeSpan.FromMinutes(1) }));
});

// Panel formlarında boş bırakılan alanlar geçerli sayılıyor; boşlar kaydetmeden önce BirkanSafe.Normalize ile "" yapılıyor.
builder.Services.AddControllersWithViews(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/hata");
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.XContentTypeOptions = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers.XFrameOptions = "SAMEORIGIN";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
