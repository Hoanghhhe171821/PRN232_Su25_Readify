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
            if (success && !String.IsNullOrWhiteSpace(data))
            {
                TempData["SuccessMessage"] = data;
                return RedirectToAction("Login");
            }
            TempData["ErrorMessage"] = errorMessage ?? "Lỗi không xác định khi đăng ký.";
            return View();
        }

        [HttpGet("logout")]
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
                return RedirectToAction("Login");
        }



    }
}
