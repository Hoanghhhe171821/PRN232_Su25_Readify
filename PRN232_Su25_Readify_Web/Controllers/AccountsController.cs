using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_Web.Models;
using PRN232_Su25_Readify_Web.Models.Account;
using PRN232_Su25_Readify_Web.Services;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string baseUrl = "https://localhost:7267/api/Accounts";

        public AccountsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 10)
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var query = $"?page={page}&pageSize={pageSize}";
            var fullUrlRoles = $"{baseUrl}/get-all-roles";


            if (!string.IsNullOrEmpty(keyword))
            {
                query += $"&keyword={Uri.EscapeDataString(keyword)}";
            }

            var fullUrl = $"{baseUrl}{query}";
            var response = await client.GetFromJsonAsync<PageResult<AccountDto>>(fullUrl);
            var roleResponse = await client.GetFromJsonAsync<List<string>>(fullUrlRoles);
            ViewBag.Roles = roleResponse;
            ViewBag.CurrentPage = page;

            return View(response ?? new PageResult<AccountDto>(
             new List<AccountDto>(), totalCount: 0, currentPage: page, pageSize: pageSize));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("setRoles")]
        public async Task<IActionResult> SetRole(string userName, List<string> roles)
        {
            var dto = new
            {
                userName = userName,
                selectedRoles = roles
            };

            var (success, data, errorMessage) = await ApiHelper.
                 PostAsync("api/Accounts/save-role", dto, _httpClientFactory);

            if (success && !String.IsNullOrWhiteSpace(data))
            {
                TempData["Success"] = data;
                return RedirectToAction("Index");
            }
            TempData["Error"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return RedirectToAction("Index");
        }
    }
}
