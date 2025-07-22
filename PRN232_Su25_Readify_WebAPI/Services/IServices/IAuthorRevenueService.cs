using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    // tong doanh thu cua author
    public interface IAuthorRevenueService
    {
        Task UpdateAuthorRevenueAsync(Book book, int amount);
    }
}
