using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Dtos;
using PRN232_Su25_Readify_Web.Dtos.TopUps;
using PRN232_Su25_Readify_Web.Models.Auth;
using PRN232_Su25_Readify_Web.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class AuthsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiClientHelper _apiClientHelper;

        public AuthsController(IHttpClientFactory httpClientFactory,ApiClientHelper apiClientHelper)
        {
            _httpClientFactory = httpClientFactory;
            _apiClientHelper = apiClientHelper;
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
            var userAgent = Request.Headers["User-Agent"].ToString();

            var (success, data, errorMessage) = await ApiHelper.PostAsync("api/auth/login", dto,
                _httpClientFactory,userAgent);
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
                    Expires = authResult.ExpriseAt
                });
                Response.Cookies.Append("session_Id", authResult.SessionsId, new CookieOptions
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
            var sessionId = Request.Cookies["session_Id"];
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // Nếu không có sessionId thì chỉ cần xóa cookie và chuyển hướng
                Response.Cookies.Delete("access_Token");
                Response.Cookies.Delete("session_Id");
                return RedirectToAction("Login", "Auths");
            }

            var userAgent = Request.Headers["User-Agent"].ToString();
            
            var (success, data, errorMessage) = await ApiHelper.PostAsync<object>("api/auth/Logout", null,
                _httpClientFactory, userAgent);
            if (success)
            {
                Response.Cookies.Delete("access_Token");
                Response.Cookies.Delete("session_Id");
            }
            else
            {
                // (Tùy chọn) Ghi log hoặc hiển thị lỗi nếu cần
                TempData["ErrorMessage"] = "Lỗi khi đăng xuất: " + errorMessage;
            }

            return RedirectToAction("Login", "Auths");
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

            var response = await client.PostAsync("https://localhost:7267/api/payment/TopUp", content);
            if (response.IsSuccessStatusCode)
            {
                // Xử lý khi thành công
                var json = await response.Content.ReadFromJsonAsync<TopUpResponseVM>();
                return RedirectToAction("ConfirmTopUp", new { id = json.TransactionId });
            }
            else
            {
                // Xử lý lỗi
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Lỗi nạp điểm: {error}";
            }

            return View();
        }

        public async Task<IActionResult> ConfirmTopUp(int id)
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://localhost:7267/api/Payment/TopUp/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể kiểm tra giao dịch.";
                return RedirectToAction("TopUpCoints");
            }

            var result = await response.Content.ReadFromJsonAsync<TopUpCheckVM>();
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> CheckTransactionStatus(int id)
        {
            var token = Request.Cookies["access_Token"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://localhost:7267/api/Payment/TopUp/{id}");
            var result = await response.Content.ReadFromJsonAsync<TopUpCheckVM>();

            if (result.Status == "SUCCESS")
            {
                TempData["SuccessMessage"] = "Thanh toán thành công! Xu đã được cộng vào tài khoản.";
                return RedirectToAction("TopUpCoints");
            }

            TempData["ErrorMessage"] = "Thanh toán chưa hoàn tất. Vui lòng thử lại sau.";
            return RedirectToAction("ConfirmTopUp", new { id });
        }
        [Authorize]
        public async Task<IActionResult> HistoryTopUp(int pageIndex = 1, int pageSize = 5)
        {
            var accessToken = Request.Cookies["access_Token"];
            var sessionId = Request.Cookies["session_Id"];
            var client = _apiClientHelper.CreateClientWithToken();

            var query = $"?pageIndex={pageIndex}&pageSize={pageSize}";
            var response = await client.GetFromJsonAsync
                <PagedResults<TopUpTransactionsDto>>($"/api/Payment/TopUp/History{query}");

            return View(response ?? new PagedResults<TopUpTransactionsDto>
            {
                Items = new List<TopUpTransactionsDto>(),
                TotalItems = 0,
                CurrentPage = pageIndex,
                PageSize = pageSize
            });

        }


    }
}
