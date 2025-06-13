using System.Threading.Tasks;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Demo.Web.Services;

namespace Demo.Web.Controllers;

public class ContactUsController : Controller
{
    private readonly string _apiBaseUrl = "http://localhost:5114/api/ContactUs/";
    private readonly HttpClient _httpClient;
    private readonly EmailService _emailService;
    public ContactUsController(IHttpClientFactory httpClientFactory, EmailService emailService)
    {
        _httpClient = httpClientFactory.CreateClient();
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
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "contact", contactUsViewModel.Email);
        bool isUserWithEmailExists = await httpResponseMessage.Content.ReadFromJsonAsync<bool>();
        if (isUserWithEmailExists)
        {
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
        else
        {
            return new JsonResult(new { success = false, message = "User with this email does not exist." });
        }
    }
}
