using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Dtos.Books;
using PRN232_Su25_Readify_Web.Models;
using PRN232_Su25_Readify_Web.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiClientHelper _apiClientHelper;
        private const string baseUrl = "https://localhost:7267/api/Cart";

        public CartController(IHttpClientFactory httpClientFactory, ApiClientHelper apiClientHelper)
        {
            _httpClientFactory = httpClientFactory;
            _apiClientHelper = apiClientHelper;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine("User Claims: " + string.Join(", ", claims));

            var accessToken = Request.Cookies["access_Token"];
            var sessionId = Request.Cookies["session_Id"];

            if(string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrEmpty(sessionId))
            {
                return RedirectToAction("Index", "home");
            }

            var client = _apiClientHelper.CreateClientWithToken();
            var query = $"?page={page}&pageSize={pageSize}";
            var response = await client.GetFromJsonAsync<PageResult<BookViewModel>>($"/api/Cart{query}");
           
            if(response != null)
            {
                var totalAmount = response.Items.Sum(t => t.Price);
                Console.WriteLine(totalAmount);
                ViewBag.TotalAmount = totalAmount;
            }

            return View(response ?? new PageResult<BookViewModel>(
             new List<BookViewModel>(), totalCount: 0, currentPage: page, pageSize: pageSize));
        }

    }
}
