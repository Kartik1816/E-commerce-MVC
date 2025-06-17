using System.Text.Json;
using Demo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class AuthController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Auth/";

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login([FromForm] AuthViewModel authViewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        HttpResponseMessage response;

        response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "login", authViewModel);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                string message = responseData.message;
                bool success = responseData.success;
                string token = responseData.token;
                string refreshToken = responseData.refreshToken;
                return new JsonResult(new { success = success, message = message, token = token, refreshToken = refreshToken });
                
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
    [Route("Auth/RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "refreshtoken", request.RefreshToken);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                string token = responseData.accessToken;
                string refreshToken = responseData.refreshToken;
                return new JsonResult(new
                {
                    token = token,
                    refreshToken = refreshToken
                });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }

        return Unauthorized();
    }
    [HttpGet]
    public IActionResult Registration()
    {
        return View();
    }

    [HttpPost]
    [Route("/Auth/Registration")] 
    public async Task<IActionResult> Registration([FromForm] RegistrationViewModel registrationViewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (registrationViewModel.Image != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(registrationViewModel.Image.FileName);
            if (!registrationViewModel.Image.ContentType.Contains("image"))
            {
                return new JsonResult(new { success = false, message = "Please upload a valid image file." });
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-images", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                registrationViewModel.Image.CopyTo(fileStream);
            }
            registrationViewModel.ImageUrl = fileName;
        }
        //temporarily remove Image of type IFormFile from view model then send
        registrationViewModel.Image = null;
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "register", registrationViewModel);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                if (responseData.success == false)
                {
                    //remove the image if registration fails
                    if (registrationViewModel.ImageUrl != null)
                    {
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-images", registrationViewModel.ImageUrl);
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
                if (registrationViewModel.ImageUrl != null)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-images", registrationViewModel.ImageUrl);
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
