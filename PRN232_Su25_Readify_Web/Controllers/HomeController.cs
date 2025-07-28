using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRN232_Su25_Readify_Web.Dtos.Home;
using PRN232_Su25_Readify_Web.Models;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        public HomeController(ILogger<HomeController> logger,IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7267/");
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var recommendBooks = await GetApiDataAsync<List<Book>>("api/Books/RecommendBooks")?? new List<Book>();
            var newReleaseBooks = await GetApiDataAsync<List<Book>>("api/Books/NewReleaseBooks") ?? new List<Book>();
           
            var data = new HomeIndexViewModel
            {
                RecommendBooks = recommendBooks,
                NewReleaseBooks = newReleaseBooks
            };
            return View(data);
        }
        //Get data by Api Url
        private async Task<T> GetApiDataAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Báo l?i n?u API tr? l?i

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
