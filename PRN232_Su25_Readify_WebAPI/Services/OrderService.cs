using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.Order;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class OrderService : IOrderService
    {

        private readonly ReadifyDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public OrderService(ReadifyDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<PagedResult<OrderDto>> GetAllOrdersAsync(int pageIndex = 1, int pageSize = 5, string? searchString = null)
        {
            var ordersQuery = _context.Order.Include(u => u.User).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearchString = searchString?.ToLowerInvariant();
                ordersQuery = ordersQuery.Where(o => o.User.Email.Contains(lowerSearchString) 
                || o.User.UserName.Contains(lowerSearchString));
            }
            var totalCount = await ordersQuery.CountAsync();

            // Lấy dữ liệu trang
            var items = await ordersQuery
            .OrderByDescending(o => o.CreateDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(o => new OrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),      // Chuyển enum -> string
                UserEmail = o.User.Email,
                CreateDate = DateTime.Parse(o.CreateDate.ToString())
            })
            .ToListAsync();

            return new PagedResult<OrderDto>
            {
                Items = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                TotalItems = totalCount
            };
        }

        public async Task<List<OrderItemDto>> GetOrderItemsAsync(int orderId)
        {
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Book).ThenInclude(b => b.Author)
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    BookId = oi.BookId,
                    UnitPrice = oi.UnitPrice,
                    BookTitle = oi.Book.Title,
                    Author = oi.Book.Author.Name,
                    ImageUrl = oi.Book.ImageUrl
                })
                .ToListAsync();
            if (orderItems.Count() == 0) throw new BRException("Order is not found");

            return orderItems;
        }

        public async Task<PagedResult<OrderDto>> GetOrdersByCustomerIdAsync(int pageIndex, int pageSize)
        {
            var userId = GetCurrentUserId();
            if (userId == null) throw new UnauthorEx("Please login to checkout");
            var ordersQuery = _context.Order
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
                .AsQueryable();

            var totalCount = ordersQuery.Count();

            var items = await ordersQuery
            .OrderByDescending(o => o.CreateDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(o => new OrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),      // Chuyển enum -> string
                UserEmail = o.User.Email,
                CreateDate = DateTime.Parse(o.CreateDate.ToString())
            })
            .ToListAsync();

            return new PagedResult<OrderDto>{
                Items = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                TotalItems = totalCount
            };
        }

        public async Task<OrderDto> GetOrderById(int orderId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) throw new UnauthorEx("Please login to checkout");

            var order = await _context.Order
                .Include(o => o.User)
                .Where(o =>  o.Id == orderId)
                .AsNoTracking()
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(), // Chuyển enum -> string
                    UserEmail = o.User.Email,
                    CreateDate = DateTime.Parse(o.CreateDate.ToString())
                })
                .FirstOrDefaultAsync();

            if (order == null) throw new BRException("Order not found");

            return order;
        }

    }
}
