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

        // ✅ Hiển thị profile
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

        // ✅ Hiển thị form chỉnh sửa
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

        // ✅ Gửi yêu cầu cập nhật thông tin
        [HttpPost]
        public async Task<IActionResult> Edit(ProfileViewModel model, IFormFile? avatar)
        {
            var client = CreateClient();
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.UserName ?? ""), "UserName");
            form.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");

            // 🔴 Thêm các trường mới
            form.Add(new StringContent(model.Bio ?? ""), "Bio");
            form.Add(new StringContent(model.PublicEmail ?? ""), "PublicEmail");
            form.Add(new StringContent(model.PublicPhone ?? ""), "PublicPhone");

            if (avatar != null)
            {
                var stream = avatar.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(avatar.ContentType);
                form.Add(content, "Avatar", avatar.FileName);
            }

            var response = await client.PutAsync("https://localhost:7267/api/Users/profile", form);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ViewBag.Error = "Cập nhật thất bại!";
            return View(model);
        }

        // ✅ Đổi mật khẩu
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
