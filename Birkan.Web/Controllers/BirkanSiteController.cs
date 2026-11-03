using Birkan.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Birkan.Web.Controllers;

public sealed class BirkanSiteController(BirkanContentStore store) : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View(store.Current);

    [HttpGet("/hata")]
    public IActionResult Error() => Problem("Beklenmeyen bir hata oluştu.");
}
