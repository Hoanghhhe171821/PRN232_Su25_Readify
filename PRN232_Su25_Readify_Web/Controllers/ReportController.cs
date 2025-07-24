using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRN232_Su25_Readify_Web.Dtos.Books;
using System.Net.Http.Headers;

public class ReportController : Controller
{
    private readonly HttpClient _httpClient;

    public ReportController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("https://localhost:7267/api/ReportError/GetAllReports");
        if (!response.IsSuccessStatusCode)
        {
            return View("Error");
        }

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<List<ChapterErrorReportDto>>(json);
        return View(data);
    }
}
