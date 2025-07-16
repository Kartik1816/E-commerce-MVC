using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class InventoryController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Inventory/";

    public InventoryController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<IActionResult> Index()
    {
        string? jwtToken = Request.Cookies["token"];
        if (string.IsNullOrEmpty(jwtToken))
        {
            return RedirectToAction("Index", "Auth");
        }

        int userId = JwtService.GetUserIdFromJwtToken(jwtToken);
        if (userId <= 0)
        {
            return RedirectToAction("Index", "Auth");
        }

        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "get-inventory-details/" + userId+ "?timeFilter=" + "");
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);

            if (responseData != null && responseData.IsSuccess)
            {
                InventoryViewModel? inventoryViewModel = JsonConvert.DeserializeObject<InventoryViewModel>(responseData.Data.ToString());
                return View(inventoryViewModel);
            }
            else
            {
                return View(new InventoryViewModel());
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while fetching inventory details.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> FilterInventory(string timeFilter,string? fromDate = null, string? toDate = null)
    {
        string? jwtToken = Request.Cookies["token"];
        if (string.IsNullOrEmpty(jwtToken))
        {
            return RedirectToAction("Index", "Auth");
        }

        int userId = JwtService.GetUserIdFromJwtToken(jwtToken);
        if (userId <= 0)
        {
            return RedirectToAction("Index", "Auth");
        }

        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "get-inventory-details/" + userId + "?timeFilter=" + timeFilter +
            (string.IsNullOrEmpty(fromDate) ? "" : "&fromDate=" + fromDate) +
            (string.IsNullOrEmpty(toDate) ? "" : "&toDate=" + toDate));
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);

            if (responseData != null && responseData.IsSuccess)
            {
                InventoryViewModel? inventoryViewModel = JsonConvert.DeserializeObject<InventoryViewModel>(responseData.Data.ToString());
                return PartialView("_InventoryPartial", inventoryViewModel);
            }
            else
            {
                return PartialView("_InventoryPartial", new InventoryViewModel());
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while filtering inventory details.");
        }

    }
}
