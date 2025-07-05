using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.Auth;
using PRN232_Su25_Readify_Web.Services;
using System.Text;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class AuthsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginDtoRequest();
            return View(model);
        }

        public async Task<IActionResult> Login(LoginDtoRequest dto)
        {
            var (success, data, errorMessage) = await ApiHelper.PostAsync("api/auth/login", dto, _httpClientFactory);
            if (success && !String.IsNullOrWhiteSpace(data))
            {
                var authResult = JsonSerializer.Deserialize<AuthResult>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                Console.WriteLine(authResult);


                Response.Cookies.Append("access_Token", authResult.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = authResult.ExpriseAt.AddMinutes(2)
                });
                Response.Cookies.Append("refresh_Token", authResult.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                });
                return RedirectToAction("Index", "Home");
            }
            TempData["ErrorMessage"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return View(dto);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDtoRequest dto)
        {
            var (success, data, errorMessage) = await ApiHelper.
                 PostAsync("api/auth/register", dto, _httpClientFactory);

            if (success && !String.IsNullOrWhiteSpace(data))
            {
                TempData["SuccessMessage"] = data;
                return RedirectToAction("login", "auths");
            }
            TempData["ErrorMessage"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return View(dto);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var model = new ForgotDtoRequest();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ForgotDtoRequest EmailForgot)
        {
            var (success, data, errorMessage) = await ApiHelper.
                PostAsync("api/auth/forgot-password", EmailForgot, _httpClientFactory);
            if (success && !String.IsNullOrWhiteSpace(data))
            {
                TempData["SuccessMessage"] = data;
                return RedirectToAction("Login");
            }
            TempData["ErrorMessage"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_Token"];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                Response.Cookies.Delete("access_Token");
                Response.Cookies.Delete("refresh_Token");
                return RedirectToAction("login");
            }

            var (success, data, errorMessage) = await ApiHelper.
                PostAsync("api/auth/logout", refreshToken, _httpClientFactory);

            if (success)
            {
                Response.Cookies.Delete("access_Token");
                Response.Cookies.Delete("refresh_Token");
            }
            return RedirectToAction("login","auths");
        }

        [HttpGet]
        public IActionResult TopUpCoints()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TopUpCoints(int points)
        {
            var token = Request.Cookies["access_Token"];
            if (string.IsNullOrWhiteSpace(token) )
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để nạp tiền";
                return View();
            }
            int[] validAmounts = { 50, 100, 200, 500 };
            if (!validAmounts.Contains(points))
            {
                TempData["ErrorMessage"] = "Chỉ cho phép nạp các mệnh giá: 50, 100, 200, 500.";
                return View();
            }
                var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new { points }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("https://localhost:7267/api/Auth/TopUp", content);
            if (response.IsSuccessStatusCode)
            {
                // Xử lý khi thành công
                TempData["SuccessMessage"] = "Nạp điểm thành công.";
            }
            else
            {
                // Xử lý lỗi
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Lỗi nạp điểm: {error}";
            }

            return View();
        }
    }
}
