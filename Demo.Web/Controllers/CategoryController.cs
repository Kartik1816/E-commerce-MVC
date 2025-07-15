using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class CategoryController : Controller
{
    private readonly string _claAPIBaseUrl = "http://localhost:5114/api/CLA/";
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Category/";
    private readonly HttpClient _httpClient;
    public CategoryController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    public async Task<IActionResult> Index()
    {

        HttpResponseMessage response = await _httpClient.GetAsync(_claAPIBaseUrl + "categories");
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseModel = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        List<CategoryViewModel>? categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(responseModel?.Data?.ToString() ?? string.Empty);
        CategoryListViewModel categoryListViewModel = new CategoryListViewModel
        {
            Categories = categories ?? new List<CategoryViewModel>()
        };
        return View(categoryListViewModel);
    }
    public async Task<IActionResult> ResetModal()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_claAPIBaseUrl + "categories");
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseModel = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        List<CategoryViewModel>? categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(responseModel?.Data?.ToString() ?? string.Empty);
        CategoryListViewModel categoryListViewModel = new CategoryListViewModel
        {
            Categories = categories ?? new List<CategoryViewModel>(),
            CategoryViewModel = new CategoryViewModel()
        };

        return PartialView("_AddEditCategoryModal", categoryListViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SaveCategory([FromForm] CategoryViewModel categoryViewModel)
    {
        if (categoryViewModel.Image != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryViewModel.Image.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/category-images", fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                categoryViewModel.Image.CopyTo(fileStream);
            }
            categoryViewModel.ImageUrl = fileName;
        }
        categoryViewModel.Image = null;
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "save-category", categoryViewModel);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                string? message = responseData.Message;
                bool success = responseData.IsSuccess;
                return new JsonResult(new { success = success, message = message });
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
    [Route("Category/DeleteCategory")]
    public async Task<IActionResult> DeleteCategory([FromForm] int id)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(_apiBaseUrl + "delete-category/" + id);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                bool success = responseData.IsSuccess;
                string? message = responseData.Message;
                return new JsonResult(new { success = success, message = message });
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

    [HttpGet]
    [Route("Category/GetCategoryById")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "get-category-by-id/" + id);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                CategoryViewModel? category = JsonConvert.DeserializeObject<CategoryViewModel>(responseData.Data?.ToString() ?? string.Empty);
                return PartialView("_AddEditCategoryModal", new CategoryListViewModel { CategoryViewModel = category });
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
    public async Task<IActionResult> ReleaseCategory(int id)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "release-category", id);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                bool success = responseData.IsSuccess;
                string? message = responseData.Message;
                return new JsonResult(new { success = success, message = message });
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
}
