using System.Text.Json;
using System.Threading.Tasks;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class EditProfileController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/EditProfile/";
    public EditProfileController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    public async Task<IActionResult> Index()
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        if (userId <= 0)
        {
            return RedirectToAction("Index", "Auth");
        }
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + userId);
        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonObject = JsonDocument.Parse(jsonString);
        var dataObject = jsonObject.RootElement.GetProperty("data");
        var profileDetails = System.Text.Json.JsonSerializer.Deserialize<EditProfileViewModel>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        if (profileDetails != null)
        {
            return View(profileDetails);
        }
        else
        {
            return RedirectToAction("Index", "Auth");
        }
    }
    [HttpPost]
    [Route("/EditProfile/SaveProfile")] 
    public async Task<IActionResult> Registration([FromForm] EditProfileViewModel editProfileViewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (editProfileViewModel.Image != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(editProfileViewModel.Image.FileName);
            if (!editProfileViewModel.Image.ContentType.Contains("image"))
            {
                return new JsonResult(new { success = false, message = "Please upload a valid image file." });
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-images", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                editProfileViewModel.Image.CopyTo(fileStream);
            }
            editProfileViewModel.ImageUrl = fileName;
        }
        //temporarily remove Image of type IFormFile from view model then send
        editProfileViewModel.Image = null;
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "saveProfile", editProfileViewModel);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                if (responseData.success == false)
                {
                    //remove the image if registration fails
                    if (editProfileViewModel.ImageUrl != null)
                    {
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-images", editProfileViewModel.ImageUrl);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                }
                string message = responseData.message;
                bool success = responseData.success;
                return new JsonResult(new { success = success, message = message });
            }
            else
            {
                if (editProfileViewModel.ImageUrl != null)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-images", editProfileViewModel.ImageUrl);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while processing the request.");
        }
    }
}
