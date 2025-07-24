using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.Account;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string apiBaseUrl = "https://localhost:7267/api/Users";

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var accessToken = HttpContext.Request.Cookies["access_Token"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync($"{apiBaseUrl}/profile");
            if (!response.IsSuccessStatusCode)
                return View(new ProfileViewModel());

            var json = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<ProfileViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(profile);
        }
    }
}
