using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly HttpClient _httpClient;
        public CategoriesController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7267/");
        }
        public IActionResult Index()
        {
            
            return View();
        }
        private async Task<T> GetApiDataAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Báo lỗi nếu API trả lỗi

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }
    }
}
