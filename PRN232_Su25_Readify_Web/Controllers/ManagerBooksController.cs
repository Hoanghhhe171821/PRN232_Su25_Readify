using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using PRN232_Su25_Readify_Web.Models.Book;
using PRN232_Su25_Readify_Web.Models.Common;

public class ManagerBooksController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string apiBase = "https://localhost:7267/api/ManagerBooks";

    public ManagerBooksController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index(int page = 1, string search = "")
    {
        var client = _httpClientFactory.CreateClient();
        var token = Request.Cookies["access_Token"];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = $"{apiBase}?page={page}&search={search}";
        var res = await client.GetAsync(url);
        if (!res.IsSuccessStatusCode) return View(new PaginatedResponse<ManagerBookReviewViewModel>());

        var json = await res.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<PaginatedResponse<ManagerBookReviewViewModel>>(json);
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
}