using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.BookReport;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class BookReportsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BookReportsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Report(int bookId, string title)
    {
        return View(new ReportBookViewModel { BookId = bookId, Title = title });
    }

    [HttpPost]
    public async Task<IActionResult> Report(ReportBookViewModel model)
    {
        var client = _httpClientFactory.CreateClient();
        var token = HttpContext.Request.Cookies["access_Token"];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var data = new
        {
            BookId = model.BookId,
            Reason = model.Reason,
            Description = model.Description
        };

        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://localhost:7267/api/BookReports", json);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Báo cáo của bạn đã được gửi.";
            return RedirectToAction("BookDetails", "Books", new { id = model.BookId });
        }

        ViewBag.Error = "Gửi báo cáo thất bại.";
        return View(model);
    }
}
