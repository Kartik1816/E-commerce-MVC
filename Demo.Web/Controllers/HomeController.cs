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
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "released-categories");
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseModel = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        List<CategoryViewModel>? categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(responseModel?.Data?.ToString() ?? string.Empty);

        //Get Top 5 Products List 
        HttpResponseMessage productResponse = await _httpClient.GetAsync(_apiBaseUrl + "GetTopFiveOfferedProducts");
        string productJsonString = await productResponse.Content.ReadAsStringAsync();
        ResponseModel? productResponseModel = JsonConvert.DeserializeObject<ResponseModel>(productJsonString);
        List<ProductViewModel>? products = JsonConvert.DeserializeObject<List<ProductViewModel>>(productResponseModel?.Data?.ToString() ?? string.Empty);

        HomeViewModel homeViewModel = new HomeViewModel
        {
            Categories = categories ?? new List<CategoryViewModel>(),
            Products = products
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
            ResponseModel? responseModelOfProfile = JsonConvert.DeserializeObject<ResponseModel>(jsonStringOfProfile);
            EditProfileViewModel? profile = JsonConvert.DeserializeObject<EditProfileViewModel>(responseModelOfProfile?.Data?.ToString() ?? string.Empty);

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
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                bool success = responseData.IsSuccess;
                if (success)
                {
                    HttpResponseMessage discountData = await _httpClient.GetAsync(_apiBaseUrl + "GetMinMaxDiscount");
                    string discount = await discountData.Content.ReadAsStringAsync();
                    ResponseModel? discountResponse = JsonConvert.DeserializeObject<ResponseModel>(discount);
                    NewSubscriberModel? newSubscriberModel = JsonConvert.DeserializeObject<NewSubscriberModel>(discountResponse?.Data?.ToString() ?? string.Empty);
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

