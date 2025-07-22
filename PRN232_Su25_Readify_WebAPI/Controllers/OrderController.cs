using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 5,
            [FromQuery] string? searchString = null)
        {
            var result = await _orderService.GetAllOrdersAsync(pageIndex, pageSize, searchString);
            return Ok(result);
        }

        [HttpGet("{orderId}/items")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            var items = await _orderService.GetOrderItemsAsync(orderId);
            return Ok(items);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrderUser([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _orderService.GetOrdersByCustomerIdAsync(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderById(id);
            return Ok(order);
        }
    }
}
