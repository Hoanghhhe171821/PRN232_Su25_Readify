using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using PRN232_Su25_Readify_Web.Models.Book;
using PRN232_Su25_Readify_Web.Models.Common;
using PRN232_Su25_Readify_Web.Dtos.DashboardAuthor;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models;
using System;
using System.Net;

public class ManagerBooksController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    private const string apiBase = "https://localhost:7267/api/ManagerBooks";

    public ManagerBooksController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(apiBase);
    }

    public async Task<IActionResult> Index(int page = 1, string search = "")
    {
        var client = _httpClientFactory.CreateClient();
        var token = Request.Cookies["access_Token"];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = $"{apiBase}?page={page}&search={search}";
        var res = await client.GetAsync(url);
        if (!res.IsSuccessStatusCode) return View(new PaginatedResponse<PRN232_Su25_Readify_Web.Models.Book.ManagerBookReviewViewModel>());

        var json = await res.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<PaginatedResponse<PRN232_Su25_Readify_Web.Models.Book.ManagerBookReviewViewModel>>(json);
        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int bookId, bool isApproved)
    {
        var client = _httpClientFactory.CreateClient();
        var token = Request.Cookies["access_Token"];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new StringContent(JsonConvert.SerializeObject(new
        {
            BookId = bookId,
            IsApproved = isApproved
        }), Encoding.UTF8, "application/json");

        var res = await client.PutAsync($"{apiBase}/decision", content);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> All(int page = 1)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{apiBase}/all?page={page}";

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Không thể tải dữ liệu từ server.";
            return View(new PaginatedWebResponse<PRN232_Su25_Readify_Web.Models.Book.ManagerBookListItemViewModel>());
        }

        var json = await response.Content.ReadAsStringAsync();

        var apiResponse = JsonConvert.DeserializeObject<PaginatedWebResponse<PRN232_Su25_Readify_Web.Models.Book.ManagerBookListItemViewModel>>(json);
        if (apiResponse == null || apiResponse.Items == null)
        {
            ViewBag.Error = "Lỗi phân tích dữ liệu từ API.";
            return View(new PaginatedWebResponse<PRN232_Su25_Readify_Web.Models.Book.ManagerBookListItemViewModel>());
        }

        return View(apiResponse);
    }
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
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            return View(model);
        }
    }
    public async Task<IActionResult> Details(int bookId)
    {
        var book = await GetAuthorizedApiDataAsync<Book>($"api/Authors/GetBookDetailsById/{bookId}");
        if (book == null) RedirectToAction("BookManager");

        var chapters = await GetApiDataAsync<List<Chapter>>($"api/Books/GetAllChapterByBookId/{bookId}");
        var bookDetails = new BookDetailsDto
        {
            Book = book,
            Chapters = chapters
        };
        return View(bookDetails);
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
    private async Task<T> GetAuthorizedApiDataAsync<T>(string apiUrl)
    {
        string fullUrl = apiBase + apiUrl;
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
        string fullUrl = apiBase + apiUrl;
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
    private HttpClient CreateClient()
    {
        var token = HttpContext.Request.Cookies["access_Token"];
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}