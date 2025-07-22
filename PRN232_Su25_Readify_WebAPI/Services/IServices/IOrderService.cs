using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.Order;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IOrderService
    {
        Task<PagedResult<OrderDto>> GetAllOrdersAsync(int pageIndex, int pageSize, string? searchString = null);
        Task<PagedResult<OrderDto>> GetOrdersByCustomerIdAsync(int pageIndex, int pageSize);
        Task<List<OrderItemDto>> GetOrderItemsAsync(int orderId);
        Task<OrderDto> GetOrderById(int orderId);
    }
}
