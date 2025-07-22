using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class BookRevenueService : IBookRevenueService
    {
        private readonly ReadifyDbContext _context;

        public BookRevenueService(ReadifyDbContext context)
        {
            _context = context; 
        }
        public Task<BookRevenueSummary> GetBookRevenueSummaryAsync(int bookId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateBookRevenueAsync(Book book, int total)
        {
            if (book == null) throw new BRException("Book is null");
            
            var summary = await _context.BookRevenueSummaries.
                FindAsync(book.Id);
            if(summary == null)
            {
                summary = new BookRevenueSummary
                {
                    BookId = book.Id,
                    TotalRevenue = total,
                    TotalSold = 1
                };
                _context.BookRevenueSummaries.Add(summary);
            }
            else
            {
                summary.TotalSold += 1;
                summary.TotalRevenue += total;
            }
        }
    }
}
