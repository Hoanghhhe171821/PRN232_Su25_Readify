using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class CartService : ICartService
    {
        private readonly ReadifyDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<AppUser> _userManager;

        public CartService(ReadifyDbContext context, IHttpContextAccessor contextAccessor,
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

        #region AddCartItem
        public async Task<string> AddCartItem(CartItem item)
        {
            var userId = GetCurrentUserId();
            if (userId == null) throw new UnauthorEx("Please login to view your cart");

            var cart = await _context.Carts.Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                cart.IsActive = true;
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            bool alreadyCartItem = await _context.CartItems.Include(ci => ci.Book)
                .AnyAsync(ci => ci.BookId == item.BookId && ci.CartId == cart.Id);
            if (alreadyCartItem)
            {
                throw new BRException("Book already in cart");
            }

            var bookFree = await _context.Books.FirstOrDefaultAsync(b => b.Id == item.BookId);
            if (bookFree == null) throw new BRException("Book is not exist");
            item.CartId = cart.Id;
            item.IsActive = true;
            if (bookFree.IsFree)
            {
                throw new BRException("Book is free");
            }
            _context.CartItems.Add(item);
            await _context.SaveChangesAsync();
            return "Item added to cart";
        }
        #endregion

        #region GetAllCart
        public async Task<PagedResult<Book>> GetAllCart(int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorEx("Please login to view your cart");

            var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Book)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return new PagedResult<Book>();

            var query = cart.CartItems.Select(ci => ci.Book
            ).AsQueryable();

            var pagedItems = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            return new PagedResult<Book>
            {
                Items = pagedItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = query.Count()
            };
        }
        #endregion

        #region RemoveCartItem
        public async Task<string> RemoveCartItem(int cartItemId)
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorEx("Please login to view your cart");

            var cartItem = await _context.CartItems.Include(ci => ci.Cart)
                            .FirstOrDefaultAsync(ci => ci.BookId == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null) throw new BRException("Cart item not found");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return "Item removed from Cart";
        }
        #endregion

        #region RemoveAllCartItem
        public async Task<bool> RemoveAllCartItem()
        {
            var userId = GetCurrentUserId();
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart.CartItems != null && cart.CartItems.Count != 0)
            {
                 _context.CartItems.RemoveRange(cart.CartItems);
                var result = await _context.SaveChangesAsync();
                if(result > 0)
                {
                    return true;
                }
            }
            return false;

        }
        #endregion





    }
}
