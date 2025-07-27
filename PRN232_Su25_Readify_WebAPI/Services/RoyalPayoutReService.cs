using Microsoft.EntityFrameworkCore;
using Octokit;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class RoyalPayoutReService : IRoyalPayoutReService
    {
        private readonly ReadifyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RoyalPayoutReService(ReadifyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCurrentAuthorId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;

            var author = _context.Authors.FirstOrDefault(a => a.UserId == userId);
            return author?.Id;
        }

        public Task<bool> ApproveRequestAsync(int requestId, int approvedAmount, string feedback)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateRequestAsync(int requestAmount)
        {
            if(requestAmount < 100000)
            {
                throw new BRException("Tiền rút ít nhất 100000");
            }

            var authorId = GetCurrentAuthorId();
            if (authorId == null)
                throw new UnauthorEx("Author not found.");
            var request = new RoyaltyPayoutRequest
            {
                RequestAmount = requestAmount,
                CreateDate = DateTime.UtcNow
            };

            var author = _context.Authors.FirstOrDefault(a => a.Id == authorId);

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);


            var totalAvailRevenue = await _context.RoyaltyTransaction
                .Where(rt =>
                    rt.AuthorId == author.Id &&
                    rt.IsPaid == false &&
                    rt.OrderItem != null &&
                    rt.OrderItem.CreateDate <= thirtyDaysAgo
                )
                .SumAsync(rt => (int?)rt.Amount) ?? 0;

            bool hasPendingRequest = await _context.RoyaltyPayoutRequests
                .AnyAsync(r => r.AuthorId == authorId && r.Status == RoyaltyPayoutStatus.Pending);

            if (hasPendingRequest)
            {
                throw new BRException("Bạn đã có yêu cầu rút tiền đang chờ xử lý. Vui lòng chờ duyệt trước khi tạo thêm.");
            }

            if (requestAmount > totalAvailRevenue)
            {
                throw new BRException("Tiền khả dụng không đủ");
            }

            request.AuthorId = authorId.Value;
            request.Status = RoyaltyPayoutStatus.Pending;
            request.ApprovedAmount = 0;


            _context.RoyaltyPayoutRequests.Add(request);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<RoyaltyPayoutRequest>> GetAllRequestsAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.RoyaltyPayoutRequests
                .Include(r => r.Author)
                .OrderByDescending(r => r.CreateDate);

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<RoyaltyPayoutRequest>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }


        public async Task<RoyaltyPayoutRequest?> GetRequestByIdAsync(int requestId)
        {
            var authorId = GetCurrentAuthorId();
            if (authorId == null) return null;

            return await _context.RoyaltyPayoutRequests
                .Where(r => r.AuthorId == authorId)
                .OrderByDescending(r => r.CreateDate)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<RoyaltyPayoutRequest>> GetRequestsByAuthorIdAsync
            (int page = 1, int pageSize = 5)
        {
            var authorId = GetCurrentAuthorId(); // Lấy từ access token
            if (authorId == null)
                throw new UnauthorizedAccessException("Author not found.");

            var query = _context.RoyaltyPayoutRequests
                .Where(r => r.AuthorId == authorId)
                .OrderByDescending(r => r.CreateDate); // Có thể sửa thành CreatedAt nếu đúng tên property

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<RoyaltyPayoutRequest>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }


        public async Task<bool> RejectRequestAsync(int requestId, string feedback)
        {
            var request = await _context.RoyaltyPayoutRequests.FindAsync(requestId);
            if (request == null || request.Status != RoyaltyPayoutStatus.Pending)
                return false;

            request.Status = RoyaltyPayoutStatus.Rejected;
            request.AdminFeedback = feedback;
            request.ApprovedAmount = 0;
            request.UpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
