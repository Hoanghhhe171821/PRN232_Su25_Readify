using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PRN232_Su25_Readify_MVC.Controllers
{
    public class ContributorRequestController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ContributorRequestController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateContributorRequestDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateContributorRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiUrl = _configuration["Backend:AppUrl"] + "/api/ContributorRequest";

                // Lấy JWT từ Session
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
                    return RedirectToAction("Login", "Account");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Chuẩn bị MultipartFormDataContent
                var content = new MultipartFormDataContent
                {
                    { new StringContent(dto.FullName ?? ""), "FullName" },
                    { new StringContent(dto.Dob.ToString("yyyy-MM-dd")), "Dob" },
                    { new StringContent(dto.CitizenId ?? ""), "CitizenId" },
                    { new StringContent(dto.PhoneNumber ?? ""), "PhoneNumber" },
                    { new StringContent(dto.Address ?? ""), "Address" },
                    { new StringContent(dto.BankAccount ?? ""), "BankAccount" },
                    { new StringContent(dto.IsAgreedToPolicy.ToString()), "IsAgreedToPolicy" }
                };

                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Gửi yêu cầu thành công!";
                    return RedirectToAction("Create");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "Lỗi: " + responseContent;
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                return View(dto);
            }
        }
    }
}
