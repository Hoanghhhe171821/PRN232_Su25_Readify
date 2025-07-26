using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.Author;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class AuthorPublicController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthorPublicController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("AuthorPublic/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync($"https://localhost:7267/api/Authors/{id}/profile");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound(); // hoặc return View("NotFound")
            }

            var json = await response.Content.ReadAsStringAsync();
            var author = JsonSerializer.Deserialize<AuthorProfileViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(author);
        }
    }
}
