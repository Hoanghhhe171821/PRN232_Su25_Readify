using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    // tong doanh cua 1 book
    public interface IBookRevenueService
    {
         Task<BookRevenueSummary> GetBookRevenueSummaryAsync(int bookId);
        Task UpdateBookRevenueAsync(Book book, int total);
    }
}
