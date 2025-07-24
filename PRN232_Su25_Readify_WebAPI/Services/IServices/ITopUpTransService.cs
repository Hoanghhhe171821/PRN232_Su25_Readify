using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.TopUpTransactions;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface ITopUpTransService
    {
        public Task<PagedResult<TopUpTransactionsDto>> GetTopUpTransactionsAsync(int pageIndex, int pageSize, string? userName = null);
    }
}
