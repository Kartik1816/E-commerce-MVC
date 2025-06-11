using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Demo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Demo.Web.Middleware;

namespace Demo.Web.Controllers;

[JwtMiddleware]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
