using Microsoft.AspNetCore.Mvc;
using Demo.Web.Middleware;
using System.Threading.Tasks;
using System.Text.Json;
using Demo.Web.Models;


namespace Demo.Web.Controllers;

[JwtMiddleware]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/CLA/";

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        
    }

    public async Task<IActionResult> Index()
    {
        HttpResponseMessage response =await _httpClient.GetAsync(_apiBaseUrl + "categories");
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
        return View(homeViewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}

