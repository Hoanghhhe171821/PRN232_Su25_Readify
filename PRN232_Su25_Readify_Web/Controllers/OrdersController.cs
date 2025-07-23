using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Dtos;
using PRN232_Su25_Readify_Web.Dtos.Order;
using PRN232_Su25_Readify_Web.Services;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiClientHelper _apiClientHelper;
        private const string baseUrl = "https://localhost:7267/api/order";

        public OrdersController(IHttpClientFactory httpClientFactory, ApiClientHelper apiClientHelper)
        {
            _apiClientHelper = apiClientHelper;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 5)
        {
            var accessToken = Request.Cookies["access_Token"];
            var sessionId = Request.Cookies["session_Id"];

            var client = _apiClientHelper.CreateClientWithToken();
            var query = $"?pageIndex={pageIndex}&pageSize={pageSize}";
            var response = await client.GetFromJsonAsync<PagedResult<OrderDto>>($"/api/order/orders/{query}");

            return View(response ?? new PagedResult<OrderDto>
            {
                Items = new List<OrderDto>(),
                TotalItems = 0,
                CurrentPage = pageIndex,
                PageSize = pageSize
            });
        }

        public async Task<IActionResult> OrderItems (int id){
            var client = _apiClientHelper.CreateClientWithToken();
            var order = await client.GetFromJsonAsync<OrderDto>($"/api/order/{id}");
            var orderItems = await client.GetFromJsonAsync<List<OrderItemDto>>($"/api/order/{id}/items");

            var viewModel = new OrderDetailsVM
            {
                Order = order,
                OrderItems = orderItems
            };

            return View(viewModel);
        }
    }
}
