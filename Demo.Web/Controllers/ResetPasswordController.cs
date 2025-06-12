using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers;

public class ResetPasswordController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
