using System.Threading.Tasks;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Demo.Web.Services;

namespace Demo.Web.Controllers;


public class ContactUsController : Controller
{

    private readonly EmailService _emailService;
    public ContactUsController(EmailService emailService)
    {
        _emailService = emailService;
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Contact([FromForm] ContactUsViewModel contactUsViewModel)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { success = false, message = "Invalid data" });
        }
        try
        {
            await _emailService.ContactUs(contactUsViewModel);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Failed to send email: " + ex.Message });
        }
        return new JsonResult(new { success = true, message = "Your message has been sent successfully." });
    }
}
