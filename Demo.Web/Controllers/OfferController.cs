using System.Text.Json;
using System.Threading.Tasks;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        ResponseModel? responseModel= JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        List<ProductViewModel>? products = JsonConvert.DeserializeObject<List<ProductViewModel>>(responseModel?.Data.ToString() ?? string.Empty);
        if (products == null || !products.Any())
        {
            return View(new CLAViewModel { Products = new List<ProductViewModel>() });
        }
        CLAViewModel productListViewModel = new CLAViewModel
        {
            Products = products
        };
        return View(productListViewModel);
    }
}
