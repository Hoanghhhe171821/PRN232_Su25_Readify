using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IRoyaltyTransactionService
    {
        Task CreateRoyaltyTransactionAsync(OrderItem orderItem);
    }
}
