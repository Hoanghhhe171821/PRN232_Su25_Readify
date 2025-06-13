using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models;
using PRN232_Su25_Readify_WebAPI.Models;
using Newtonsoft.Json;
using PRN232_Su25_Readify_Web.Dtos;

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
            var responseRecommendBooks = await _httpClient.GetAsync("api/Books/RecommendBooks");
            var jsonRecommendBooks = await responseRecommendBooks.Content.ReadAsStringAsync();
            var recommendBooks = JsonConvert.DeserializeObject<List<Book>>(jsonRecommendBooks);

            var responseNewReleaseBooks = await _httpClient.GetAsync("api/Books/NewReleaseBooks");
            var jsonNewReleaseBooks = await responseNewReleaseBooks.Content.ReadAsStringAsync();
            var newReleaseBooks = JsonConvert.DeserializeObject<List<Book>>(jsonNewReleaseBooks);


            var data = new HomeIndexViewModel
            {
                RecommendBooks = recommendBooks,
                NewReleaseBooks = newReleaseBooks

            };
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
