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

        public CheckoutService(ReadifyDbContext context, IHttpContextAccessor contextAccessor,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
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

            if(listBookAuthor.Count() > 0)
            {
                Console.WriteLine(listBookAuthor.Count());
            }

            return false;
        }

    }
}
