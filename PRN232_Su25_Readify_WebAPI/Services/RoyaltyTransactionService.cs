using Microsoft.EntityFrameworkCore;
using Octokit;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class RoyaltyTransactionService : IRoyaltyTransactionService
    {
        private readonly ReadifyDbContext _context;

        public RoyaltyTransactionService(ReadifyDbContext context)
        {
            _context = context;
        }
        public async Task CreateRoyaltyTransactionAsync(OrderItem orderItem)
        {
            if(orderItem == null) throw new ArgumentNullException(nameof(orderItem));
            if(orderItem.Book == null)
            {
                orderItem.Book = await _context.Books.Include(b => b.Author)
                    .ThenInclude(a => a.User)
                    .FirstOrDefaultAsync(b => b.Id == orderItem.BookId)
                ?? throw new InvalidOperationException($"Book {orderItem.BookId} not found.");
            }
            int royalty = 0;
            if (orderItem.Book.Author.UserId != null)
            {
                 royalty = (int)Math.Round(orderItem.Book.Price * 
                    ((decimal)(100 - orderItem.Book.RoyaltyRate) / 100m),
                  MidpointRounding.AwayFromZero);
            }

            var rt = new RoyaltyTransaction
            {
                BookId = orderItem.BookId,
                AuthorId = orderItem.Book.AuthorId,
                IsPaid = orderItem.Book.Author.UserId == null ? true : false,
                Amount = orderItem.Book.Author.UserId == null ? orderItem.UnitPrice : royalty,
                OrderItemId = orderItem.Id,
                CreateDate = DateTime.UtcNow
            };
            await _context.RoyaltyTransaction.AddAsync(rt);
        }
    }
}
