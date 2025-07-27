using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;
using PRN232_Su25_Readify_Web.Models.BookReport;
using PRN232_Su25_Readify_WebAPI.Dtos.Reports;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class ReportManagerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiBaseUrl = "https://localhost:7267/api/BookReports";

        public ReportManagerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateClient();
            var response = await client.GetAsync("https://localhost:7267/api/Reports/manager");

            if (!response.IsSuccessStatusCode)
                return View(new List<ReportManagementViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var reports = JsonSerializer.Deserialize<List<ReportManagementViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(reports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = CreateClient();
            var response = await client.GetAsync($"https://localhost:7267/api/Reports/{id}/detail");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var report = JsonSerializer.Deserialize<ReportManagementViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(report);
        }

        [HttpPost]
        public async Task<IActionResult> Handle(HandleReportDto dto)
        {
            var client = CreateClient();
            var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{apiBaseUrl}/handle", jsonContent);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ViewBag.Error = "Xử lý thất bại!";
            return RedirectToAction("Details", new { id = dto.ReportId });
        }

        [HttpPost]
        public async Task<IActionResult> FinalDecision(int ReportId, bool AcceptAuthorDenial, string Note)
        {
            var client = CreateClient();

            var data = new
            {
                ReportId = ReportId,
                AcceptAuthorDenial = AcceptAuthorDenial,
                Note = Note
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var res = await client.PutAsync("https://localhost:7267/api/BookReports/manager-decision", jsonContent);

            if (res.IsSuccessStatusCode)
            {
                TempData["Success"] = "Đã cập nhật quyết định.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Không thể cập nhật quyết định.";
            return RedirectToAction("Details", new { id = ReportId });
        }

        [HttpGet("ReviewDenial/{id}")]
        public async Task<IActionResult> ReviewDenial(int id)
        {
            var client = CreateClient();
            var res = await client.GetAsync($"https://localhost:7267/api/BookReports/{id}/author-denied");

            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<ManagerDecisionViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(model);
        }

        [HttpPost("ReviewDenial")]
        public async Task<IActionResult> ReviewDenial(ManagerDecisionViewModel model)
        {
            var client = CreateClient();

            var data = new
            {
                ReportId = model.ReportId,
                AcceptAuthorDenial = model.AcceptAuthorDenial,
                Note = model.Note
            };

            var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var res = await client.PutAsync("https://localhost:7267/api/BookReports/manager-decision", json);

            if (res.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ViewBag.Error = "Không thể gửi quyết định.";
            return View(model);
        }

    }
}
