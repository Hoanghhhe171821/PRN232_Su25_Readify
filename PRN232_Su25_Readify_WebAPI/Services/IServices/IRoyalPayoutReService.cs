using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IRoyalPayoutReService
    {
        Task<PagedResult<RoyaltyPayoutRequest>> GetAllRequestsAsync(int page = 1, int pageSize = 10); // Admin
        Task<PagedResult<RoyaltyPayoutRequest>> GetRequestsByAuthorIdAsync(int page = 1, int pageSize = 5); // Author
        Task<RoyaltyPayoutRequest?> GetRequestByIdAsync(int requestId); // Chi tiết request

        Task<bool> CreateRequestAsync(int requestAmount); // Author tạo yêu cầu
        Task<bool> ApproveRequestAsync(int requestId, int approvedAmount, string feedback); // Admin duyệt
        Task<bool> RejectRequestAsync(int requestId, string feedback); // Admin từ chối
    }
}
