using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Dtos;
using PRN232_Su25_Readify_Web.Dtos.Books;
using PRN232_Su25_Readify_Web.Models.Author;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class AuthorPublicController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string baseUrl = "https://localhost:7267/api/Authors";

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


        public async Task<IActionResult> AuthorBookRevenue(int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var token = Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var query = $"?pageNumber={page}&pageSize={pageSize}";
            var fullUrl = $"{baseUrl}/book-revenue{query}";

            var response = await client.GetFromJsonAsync<PagedResults<BookRevenueDto>>(fullUrl);

            return View("GetBookRevenue", response ?? new PagedResults<BookRevenueDto>
            {
                Items = new List<BookRevenueDto>(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = 0
            });
        }

        public async Task<IActionResult> BookTransactions(int bookId, int pageNumber = 1, int pageSize = 5)
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var query = $"?pageNumber={pageNumber}&pageSize={pageSize}";
            var fullUrl = $"{baseUrl}/book-revenue/{bookId}/transactions{query}";

            var response = await client.GetFromJsonAsync<PagedResults<RoyaltyTransactionDto>>(fullUrl);
            ViewBag.BookId = bookId; // Lưu bookId vào ViewBag để sử dụng trong view

            return View(response ?? new PagedResults<RoyaltyTransactionDto>
            {
                Items = new List<RoyaltyTransactionDto>(),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = 0
            });
        }


    }
}
