using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.Auth;
using PRN232_Su25_Readify_Web.Services;
using System.Text;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{

    public class AuthController : Controller
    {
        private const string apiBackend = "https://localhost:7267/api/auth/";
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private JwtPayLoad DecodeJwt(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '='); // padding

            var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            var json = Encoding.UTF8.GetString(jsonBytes);

            var payloadData = JsonSerializer.Deserialize<JwtPayLoad>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return payloadData;
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            var model = new LoginDtoRequest();
            return View(model);
        }
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDtoRequest dto)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(apiBackend);
            var (success, data, errorMessage) = await ApiHelper.PostAsync("api/auth/login", dto, _httpClientFactory);
            if (success && !String.IsNullOrWhiteSpace(data))
            {
                var authResult = JsonSerializer.Deserialize<AuthResult>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var token = authResult?.Data;
                Console.WriteLine(token);
                Response.Cookies.Append("accessToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("login", "auth");
            }
            TempData["ErrorMessage"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return View(dto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDtoRequest dto)
        {
            var (success, data, errorMessage) = await ApiHelper.
                 PostAsync("api/auth/register", dto, _httpClientFactory);

            if (success && !String.IsNullOrWhiteSpace(data))
            {
                TempData["SuccessMessage"] = data;
                return View();

            }
            TempData["ErrorMessage"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return View(dto);
        }
        [HttpGet("reset-password")]
        public IActionResult ResetPassword()
        {
            var model = new ForgotDtoRequest();
            return View(model);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ForgotDtoRequest EmailForgot)
        {
            var (success, data, errorMessage) = await ApiHelper.
                PostAsync("api/auth/forgot-password", EmailForgot, _httpClientFactory);
            if(success && !String.IsNullOrWhiteSpace(data))
            {
                TempData["SuccessMessage"] = data;
                return RedirectToAction("Login");
            }
            TempData["ErrorMessage"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return View();
        }

    }
}
