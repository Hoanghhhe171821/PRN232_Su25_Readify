using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.Account;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string apiBaseUrl = "https://localhost:7267/api/Users";

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            var token = HttpContext.Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateClient();
            var response = await client.GetAsync($"{apiBaseUrl}/profile");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Login", "Accounts");

            var json = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<ProfileViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var client = CreateClient();
            var response = await client.GetAsync($"{apiBaseUrl}/profile");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<ProfileViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProfileViewModel model, IFormFile? avatar)
        {
            var client = CreateClient();
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.UserName ?? ""), "UserName");
            form.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
            if (avatar != null)
            {
                form.Add(new StreamContent(avatar.OpenReadStream()), "Avatar", avatar.FileName);
            }

            var response = await client.PutAsync($"{apiBaseUrl}/profile", form);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ViewBag.Error = "Cập nhật thất bại!";
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                return View(model);
            }

            var client = CreateClient();
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{apiBaseUrl}/change-password", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            ViewBag.Error = "Lỗi: " + errorJson;
            return View(model);
        }
    }
}
