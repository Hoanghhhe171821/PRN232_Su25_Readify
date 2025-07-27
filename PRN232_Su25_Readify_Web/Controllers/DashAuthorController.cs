using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Dtos;
using PRN232_Su25_Readify_Web.Dtos.DashboardAuthor;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PRN232_Su25_Readify_WebAPI.Models;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class DashAuthorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly string _uri = "https://localhost:7267/";
        public DashAuthorController(IHttpClientFactory factory)
        {
            _httpClientFactory = factory;
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri(_uri);
        }
        private HttpClient CreateClient()
        {
            var token = HttpContext.Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            RevenueSummaryViewModel revenueData = null;

            var response = await client.GetAsync("https://localhost:7267/api/Authors/revenue-summary");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                revenueData = System.Text.Json.JsonSerializer.Deserialize<RevenueSummaryViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return View(revenueData);
        }

        public async Task<IActionResult> BookManager(int pageNumber = 1, int pageSize = 7)
        {
            var responseJson = await GetAuthorizedApiDataAsync<JObject>(
                $"api/Authors/GetAllBooksByAuthor?pageNumber={pageNumber}");

            if (responseJson == null)
            {
                return View(new PagedResults<Book>
                {
                    Items = new List<Book>(),
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = 0
                });
            }

            var pagedResult = new PagedResults<Book>();

            pagedResult.CurrentPage = responseJson.Value<int?>("currentPage") ?? 1;
            pagedResult.PageSize = responseJson.Value<int?>("pageSize") ?? pageSize;
            pagedResult.TotalItems = responseJson.Value<int?>("totalBooks") ?? 0;

            var booksToken = responseJson["books"];
            if (booksToken != null)
            {
                pagedResult.Items = booksToken.ToObject<List<Book>>() ?? new List<Book>();
            }
            else
            {
                pagedResult.Items = new List<Book>();
            }

            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await GetApiDataAsync<List<Category>>("api/Categories/GetAllCategories");
            ViewBag.Categories = categories ?? new List<Category>();

            var model = new CreateBookDto(); 
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateBookDto model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                var categoriesOnError = await GetApiDataAsync<List<Category>>("api/Categories/GetAllCategories");
                ViewBag.Categories = categoriesOnError ?? new List<Category>();
                return View(model);
            }
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Ảnh không được để trống.";
                var categoriesOnError = await GetApiDataAsync<List<Category>>("api/Categories/GetAllCategories");
                ViewBag.Categories = categoriesOnError ?? new List<Category>();
                return View(model);
            }
            var client = CreateClient();

            // Gửi ảnh sang API (Multipart)
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageFile.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
            content.Add(streamContent, "ImageFile", imageFile.FileName);
            content.Add(new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"), "bookData");

            try
            {
                // Gửi MultipartFormDataContent đúng kiểu
                var result = await PostAuthorizedApiDataAsync<Book>("api/Authors/CreateBook", content);

                TempData["Success"] = "Tạo sách thành công! ";

                return RedirectToAction("BookManager");
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> CreateRequestRoyal()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequestRoyal(int RequestAmount)
        {
            var token = Request.Cookies["access_Token"];
           
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                RequestAmount.ToString(),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("https://localhost:7267/api/Authors/royal-payout-request", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Yêu cầu rút tiền đã được gửi thành công!";
                return RedirectToAction("CreateRequestRoyal");
            }

            var errorContent = await response.Content.ReadAsStringAsync();

            // Nếu nội dung là JSON chứa field "message", deserialize để lấy ra
            try
            {
                using var document = JsonDocument.Parse(errorContent);
                var root = document.RootElement;
                if (root.TryGetProperty("message", out var messageElement))
                {
                    TempData["ErrorMessage"] = messageElement.GetString();
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra.";
            }
            return View();
        }

        public async Task<IActionResult> ListPayoutRequest(int pageIndex = 1, int pageSize = 5)
        {
            var token = Request.Cookies["access_Token"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var query = $"page={pageIndex}&pageSize={pageSize}";
            var response = await client.GetFromJsonAsync<PagedResults<RoyaltyRequestDto>>(
                $"https://localhost:7267/api/Authors/author-request-pay?{query}");

            return View(response ?? new PagedResults<RoyaltyRequestDto>
            {
                Items = new List<RoyaltyRequestDto>(),
                TotalItems = 0,
                CurrentPage = pageIndex,
                PageSize = pageSize
            });

        }
        private async Task<T> GetApiDataAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            response.EnsureSuccessStatusCode(); // Báo lỗi nếu API trả lỗi

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }
        private async Task<T> PostApiDataAsync<T>(string url, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, contentData);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            var resultData = JsonConvert.DeserializeObject<T>(resultJson);

            return resultData;
        }
        private async Task<T> GetAuthorizedApiDataAsync<T>(string apiUrl)
        {
            string fullUrl = _uri + apiUrl;
            var token = Request.Cookies["access_Token"];
            if (string.IsNullOrEmpty(token)) return default;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(fullUrl);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }
        private async Task<T> PostAuthorizedApiDataAsync<T>(string apiUrl, HttpContent bodyContent)
        {
            string fullUrl = _uri + apiUrl;
            var token = Request.Cookies["access_Token"];
            if (string.IsNullOrEmpty(token)) return default;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(fullUrl, bodyContent);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            var resultData = JsonConvert.DeserializeObject<T>(resultJson);

            return resultData;
        }
    }
}
