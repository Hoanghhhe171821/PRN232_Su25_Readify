using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class AuthorRevenueService : IAuthorRevenueService
    {
        private readonly ReadifyDbContext _context;

        public AuthorRevenueService(ReadifyDbContext context)
        {
            _context = context;
        }


        public async Task UpdateAuthorRevenueAsync(Book book, int amount)
        {
            if (book == null) throw new BRException("Book is null");
            if (book.RoyaltyRate < 0) throw new BRException("Book don't have royalty rate");

            if (book.Author == null)
            {
                book.Author = await _context.Authors.Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == book.AuthorId)
                    ?? throw new NFoundEx($"Author {book.AuthorId} not found.");
            }
            if (book.Author.User == null) return;

            int royalty = (int)Math.Round(amount * ((decimal) book.RoyaltyRate / 100m),
                              MidpointRounding.AwayFromZero);
            if (royalty <= 0) return;

            var summary = await _context.AuthorRevenueSummary.FindAsync(book.AuthorId);
            if (summary == null)
            {
                summary = new AuthorRevenueSummary
                {
                    AuthorId = book.AuthorId,
                    TotalRevenue = royalty,
                    TotalPaid = 0
                };
                _context.AuthorRevenueSummary.Add(summary);
            }
            else
            {
                summary.TotalRevenue = royalty;
            }
        }


    }
}
