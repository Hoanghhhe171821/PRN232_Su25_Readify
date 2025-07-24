using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.TopUpTransactions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class TopUpTransService : ITopUpTransService
    {
        private readonly ReadifyDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<AppUser> _userManager;

        public TopUpTransService(ReadifyDbContext context,IHttpContextAccessor contextAccessor,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }
        private string GetCurrentUserId()
        {
            return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<PagedResult<TopUpTransactionsDto>> GetTopUpTransactionsAsync(int pageIndex, int pageSize, string? userName = null)
        {
            var query = _context.TopUpTransactions
                .Include(t => t.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(userName))
            {
                var norm = userName.Trim().ToUpperInvariant();
                query = query.Where(t =>
                    t.User != null &&
                    t.User.NormalizedUserName != null &&
                    t.User.NormalizedUserName.Contains(norm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TopUpTransactionsDto
                {
                    Id = t.Id,
                    UserName = t.User != null ? t.User.UserName : string.Empty,
                    Points = t.Points,
                    Amount = t.Amount,
                    MoMoOrderId = t.MoMoOrderId,
                    MomoRequestId = t.MoMoRequestId,
                    QrCodeUrl = t.QrCodeUrl,
                    PaymentUrl = t.PaymentUrl,
                    Status = t.Status.ToString(),   // nếu enum
                    CreateDate = t.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<TopUpTransactionsDto>
            {
                Items = items,
                TotalItems = totalCount,
                CurrentPage = pageIndex,
                PageSize = pageSize
            };
        }
    }
}
