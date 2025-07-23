using PRN232_Su25_Readify_WebAPI.Dtos.TopUpCoints;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IPaymentService
    {
        Task<TopUpResponse> CreateMoMoOrder(int points, string userId);
        Task<string> CheckMoMoTransaction(string orderId);
    }
}
