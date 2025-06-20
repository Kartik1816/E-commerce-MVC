using System.Text.Json;
using System.Threading.Tasks;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers;

public class OfferController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/CLA/";

    public OfferController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    public async Task<IActionResult> Index()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "GetOfferedProducts");
        string jsonString = await response.Content.ReadAsStringAsync();
        JsonDocument jsonObject = JsonDocument.Parse(jsonString);
        JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
        List<ProductViewModel>? products = System.Text.Json.JsonSerializer.Deserialize<List<ProductViewModel>>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        CLAViewModel productListViewModel = new CLAViewModel
        {
            Products = products ?? new List<ProductViewModel>(),
        };
        return View(productListViewModel);
    }
}
