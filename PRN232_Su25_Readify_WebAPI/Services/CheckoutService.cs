using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Octokit;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ReadifyDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRoyaltyTransactionService _royaltyTransaction;
        private readonly IAuthorRevenueService _authorRevenue;
        private readonly IBookRevenueService _bookRevenue;

        public CheckoutService(ReadifyDbContext context, IHttpContextAccessor contextAccessor,
            IBookRevenueService bookRevenue,
            IAuthorRevenueService authorRevenue,
            IRoyaltyTransactionService royaltyTransaction,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _royaltyTransaction = royaltyTransaction;
            _authorRevenue = authorRevenue;
            _bookRevenue = bookRevenue;
        }

        private string GetCurrentUserId()
        {
            return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<bool> Checkout()
        {
            var userId = GetCurrentUserId();
            if (userId == null) throw new UnauthorEx("Please login to checkout");

            var cart = await _context.Carts.Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Book).ThenInclude(b => b.Author)
                            .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) throw new BRException("Please add cart item to checkout");

            var listBookBuy = cart.CartItems.Select(ci => ci.Book).ToList();

            var listBookAuthor = listBookBuy.Where(a => a.Author.UserId != null).ToList();

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.Now,
                    Status = StatusOrder.Completed
                };

                _context.Order.Add(order);
                await _context.SaveChangesAsync();
                int totalAmount = 0;
                foreach (var item in listBookBuy)
                {
                    var book = item;
                    var unitPrice = item.Price;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        BookId = item.Id,
                        Quantity = 1,
                        UnitPrice = unitPrice
                    };
                    _context.OrderItems.Add(orderItem);
                    totalAmount += unitPrice;
                    await _context.SaveChangesAsync();

                    await _royaltyTransaction.CreateRoyaltyTransactionAsync(orderItem);
                    await _bookRevenue.UpdateBookRevenueAsync(book, unitPrice);
                    await _authorRevenue.UpdateAuthorRevenueAsync(book,unitPrice);
                    await _context.SaveChangesAsync();
                }
                _context.Database.CommitTransaction();

            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new BRException(ex.ToString());
            }
            return false;
        }

    }
}
