using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.BookReport;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public class AuthorReportsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string api = "https://localhost:7267/api/BookReports";

    public AuthorReportsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Warning(int id)
    {
        var client = CreateClient();
        var reportRes = await client.GetAsync($"{api}/warning/{id}");
        if (!reportRes.IsSuccessStatusCode) return NotFound();

        var json = await reportRes.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<AuthorReportViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Warning(AuthorReportViewModel model)
    {
        var client = CreateClient();

        var data = new
        {
            ReportId = model.ReportId,
            AcceptLock = model.AcceptLock,
            Reason = model.DenialReason
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var res = await client.PutAsync($"{api}/author-response", jsonContent);

        if (res.IsSuccessStatusCode)
            return RedirectToAction("PendingWarnings", "AuthorReports");

        ViewBag.Error = "Không thể gửi phản hồi.";
        return View(model);
    }
    public async Task<IActionResult> PendingWarnings()
    {
        var client = CreateClient();
        var res = await client.GetAsync($"{api}/pending-warnings");

        if (!res.IsSuccessStatusCode)
        {
            ViewBag.Error = "Không thể tải danh sách cảnh báo.";
            return View(new List<AuthorPendingWarningViewModel>());
        }

        var json = await res.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<AuthorPendingWarningViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(list);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        var token = HttpContext.Request.Cookies["access_Token"];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
