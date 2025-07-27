using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Dtos;
using PRN232_Su25_Readify_Web.Dtos.DashboardAuthor;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class DashAuthorController : Controller
    {
        private readonly IHttpClientFactory _httpClient;

        public DashAuthorController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            RevenueSummaryViewModel revenueData = null;

            var response = await client.GetAsync("https://localhost:7267/api/Authors/revenue-summary");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                revenueData = JsonSerializer.Deserialize<RevenueSummaryViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return View(revenueData);
        }

        public async Task<IActionResult> CreateRequestRoyal()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequestRoyal(int RequestAmount)
        {
            var token = Request.Cookies["access_Token"];
           
            var client = _httpClient.CreateClient();
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

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var query = $"page={pageIndex}&pageSize={pageSize}";
            var response = await client.GetFromJsonAsync<PagedResult<RoyaltyRequestDto>>(
                $"https://localhost:7267/api/Authors/author-request-pay?{query}");

            return View(response ?? new PagedResult<RoyaltyRequestDto>
            {
                Items = new List<RoyaltyRequestDto>(),
                TotalItems = 0,
                CurrentPage = pageIndex,
                PageSize = pageSize
            });

        }

    }
}
