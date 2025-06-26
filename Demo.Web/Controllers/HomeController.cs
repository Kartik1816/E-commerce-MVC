using Microsoft.AspNetCore.Mvc;
using Demo.Web.Middleware;
using System.Threading.Tasks;
using System.Text.Json;
using Demo.Web.Models;
using Demo.Web.Services;
using System.Net.Mail;
using Newtonsoft.Json;


namespace Demo.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/CLA/";
    private readonly EmailService _emailService;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, EmailService emailService)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _emailService = emailService;

    }

    public async Task<IActionResult> Index()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "categories");
        string jsonString = await response.Content.ReadAsStringAsync();
        JsonDocument jsonObject = JsonDocument.Parse(jsonString);
        JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
        List<CategoryViewModel>? categories = System.Text.Json.JsonSerializer.Deserialize<List<CategoryViewModel>>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        HomeViewModel homeViewModel = new HomeViewModel
        {
            Categories = categories ?? new List<CategoryViewModel>(),
        };
        string? token = Request.Cookies["token"];
        if (token != null)
        {
            string role = JwtService.GetRoleFromJwtToken(token);
            HttpContext.Session.SetString("UserRole", role);

            int userId = JwtService.GetUserIdFromJwtToken(token);

            if (userId > 0)
            {
                HttpContext.Session.SetInt32("UserId", userId);
            }


            string _apiBaseUrlForProfile = "http://localhost:5114/api/EditProfile/";
            HttpResponseMessage profileData = await _httpClient.GetAsync(_apiBaseUrlForProfile + userId);
            string jsonStringOfProfile = await profileData.Content.ReadAsStringAsync();
            JsonDocument jsonObjectOfProfile = JsonDocument.Parse(jsonStringOfProfile);
            JsonElement dataObjectofProfile = jsonObjectOfProfile.RootElement.GetProperty("data");
            EditProfileViewModel? profile = System.Text.Json.JsonSerializer.Deserialize<EditProfileViewModel>(
                dataObjectofProfile.ToString(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (profile?.ImageUrl != null)
            {
                if (!string.IsNullOrEmpty(profile?.ImageUrl))
                {
                    HttpContext.Session.SetString("ImageUrl", profile.ImageUrl);
                }
            }
        }
        return View(homeViewModel);

    }

    [HttpPost]
    public async Task<IActionResult> SubscribeNewUser(string email)
    {
        MailAddress mailAddress = new System.Net.Mail.MailAddress(email);

        if (mailAddress.Address != email)
        {
            return new JsonResult(new { success = true, message = "Inavalid email format" });
        }

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "subscribe-user", email);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                bool success = responseData.success;
                if (success)
                {
                    HttpResponseMessage discountData = await _httpClient.GetAsync(_apiBaseUrl + "GetMinMaxDiscount");
                    string discount = await discountData.Content.ReadAsStringAsync();
                    JsonDocument discountDocument = JsonDocument.Parse(discount);
                    JsonElement objectOfDiscount = discountDocument.RootElement.GetProperty("data");
                    NewSubscriberModel? newSubscriberModel = System.Text.Json.JsonSerializer.Deserialize<NewSubscriberModel>(
                        objectOfDiscount.ToString(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );
                    if (newSubscriberModel != null)
                    {
                        try
                        {
                            await _emailService.NewSubscriberMail(email, newSubscriberModel);
                            return new JsonResult(new { success = true, message = "Offer mail sent successfully" });
                        }
                        catch (Exception e)
                        {
                            throw new Exception("An Exception occured while sending offer mail" + e);
                        }
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = "An Exception occured while sending offer mail" });
                    }
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Subscribed User not found or Invalid email address" });
                }

            }
            else
            {
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while processing the request.");
        }

    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new{success=true,message="session cleared"});
    }

}

