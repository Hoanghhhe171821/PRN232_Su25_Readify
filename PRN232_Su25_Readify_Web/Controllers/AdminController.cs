using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Dtos;
using PRN232_Su25_Readify_Web.Dtos.Admin;
using PRN232_Su25_Readify_Web.Dtos.DashboardAuthor;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        public AdminController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            RevenueSummaryViewModel revenueData = null;

            var response = await client.GetAsync("https://localhost:7267/api/Accounts/admin/revenue-summary");
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

        public async Task<IActionResult> ListRoyaltyRequests(int page = 1, int pageSize = 10)
        {
            var token = Request.Cookies["access_Token"];

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetFromJsonAsync<PagedResults<RoyaltyPayoutRequest>>(
                $"https://localhost:7267/api/Authors/royal-request-all?page={page}&pageSize={pageSize}");

            var viewModel = new PagedResults<RoyaltyRequestAdminViewModel>
            {
                Items = response.Items.Select(x => new RoyaltyRequestAdminViewModel
                {
                    Id = x.Id,
                    AuthorName = x.Author?.Name,
                    RequestAmount = x.RequestAmount,
                    CreateDate = x.CreateDate?? DateTime.UtcNow,
                    Status = (int) x.Status,
                }).ToList(),
                TotalItems = response.TotalItems,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(viewModel);
        }




    }
}
