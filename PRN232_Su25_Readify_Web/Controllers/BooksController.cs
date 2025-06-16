using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_Web.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace PRN232_Su25_Readify_Web.Controllers
{
    [Route("[Controller]")]
    public class BooksController : Controller
    {
        private readonly HttpClient _httpClient;
        public BooksController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7267/");
        }
        [HttpGet("BookList")]
        public async Task<IActionResult> BookList(int page = 1)
        {
            var booksJsonResult = await GetApiDataAsync<JObject>($"api/Books/GetAllBooks?page={page}");

            var books = booksJsonResult["items"].ToObject<List<Book>>();
            var totalItems = booksJsonResult["totalItems"].ToObject<int>();
            var pageSize = booksJsonResult["pageSize"].ToObject<int>();
            var totalPage = (int)Math.Ceiling((double)totalItems / pageSize);

            var categories = await GetApiDataAsync<List<Category>>("api/Categories/GetAllCategories");

            var model = new BookListViewModel
            {
                PagedBooks = new PagedResult<Book>
                {
                    Items = books,
                    TotalItems = totalItems,
                    PageSize = pageSize,
                    PageNumber = page,
                    TotalPage = totalPage
                },
                Categories = categories.ToList()

            };
            return View(model);
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
