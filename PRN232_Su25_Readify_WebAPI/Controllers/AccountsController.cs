using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.Authors;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Dtos.RoyaltyPayout;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ReadifyDbContext _context;
        private readonly IMail _mailService;

        public AccountsController(IAuthService authService,
            UserManager<AppUser> userManager,
            IJwtService jwtService,
            ReadifyDbContext context,
            RoleManager<IdentityRole> roleManager,
            IMail mailService)
        {
            _authService = authService;
            _userManager = userManager;
            _jwtService = jwtService;
            _roleManager = roleManager;
            _context = context;
            _mailService = mailService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllAccounts([FromQuery] string? keyword,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var accounts = await _authService.ListAccountsAsync(keyword, page, pageSize);

            return Ok(accounts);
        }

        [HttpGet("get-all-roles")]
        public async Task<IActionResult> GetAllRole()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Ok(roles);
        }

        [HttpPost("save-role")]
        public async Task<IActionResult> SetRoles([FromBody] AssignRoleRequest dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null) throw new BRException("User is not found");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var invalidRoles = dto.SelectedRoles.Except(allRoles).ToList();
            if (invalidRoles.Any())
            {
                return BadRequest(new { message = $"Các role không hợp lệ: {string.Join(", ", invalidRoles)}" });
            }
            var resultRemove = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var resultAdd = await _userManager.AddToRolesAsync(user, dto.SelectedRoles);

            return Ok("Add Role Successfull");
        }


        [HttpPost("approvel-royalty-request")]
        public async Task<IActionResult> ApproveRoyaltyRequest([FromBody] RoyaltyRequestActionDto dto)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new UnauthorEx("Không xác định được người dùng từ token");

            var request = await _context.RoyaltyPayoutRequests
                .Include(r => r.Author)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            var revenSummary = await _context.AuthorRevenueSummary
                .FirstOrDefaultAsync(au => au.AuthorId == request.AuthorId);

            if (request == null)
                return NotFound();

            if (request.Status != RoyaltyPayoutStatus.Pending)
                return BadRequest("Yêu cầu này đã được xử lý.");

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var availableTransactions = await _context.RoyaltyTransaction
                .Where(rt =>
                    rt.AuthorId == request.AuthorId &&
                    !rt.IsPaid &&
                    rt.OrderItem != null &&
                    rt.OrderItem.CreateDate <= thirtyDaysAgo)
                .OrderBy(rt => rt.Amount)
                .ToListAsync();

            var selectedTransactions = new List<RoyaltyTransaction>();
            decimal totalSelected = 0;

            foreach (var transaction in availableTransactions)
            {
                if (totalSelected == 0 && transaction.Amount >= request.RequestAmount)
                {
                    // Trường hợp transaction đầu tiên đủ tiền -> chọn luôn
                    selectedTransactions.Add(transaction);
                    totalSelected = transaction.Amount;
                    break;
                }

                if (totalSelected + transaction.Amount <= request.RequestAmount)
                {
                    selectedTransactions.Add(transaction);
                    totalSelected += transaction.Amount;

                    if (totalSelected >= request.RequestAmount)
                        break;
                }
            }

            if (selectedTransactions.Count == 0)
                return BadRequest("Không tìm thấy các transaction phù hợp để xử lý.");

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var transaction in selectedTransactions)
                {
                    transaction.IsPaid = true;

                    var payoutTransaction = new RoyaltyPayoutTransaction
                    {
                        RoyaltyTransactionId = transaction.Id,
                        CreateDate = DateTime.UtcNow,
                        RoyaltyPayoutRequestId = request.Id
                    };
                    _context.RoyaltyPayoutTransaction.Add(payoutTransaction);
                }

                request.Status = RoyaltyPayoutStatus.Approved;
                request.AdminFeedback = dto.Message;
                request.UpdateDate = DateTime.Now;
                request.ApprovedAmount = (int)totalSelected;
                request.CreateDate = DateTime.Now;

                if (revenSummary != null)
                {
                    revenSummary.TotalPaid += (int)totalSelected;
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.Author.UserId);

                if (user != null)
                {
                    await _mailService.SendRoyaltyApprovedMailAsync(user.Email, user.UserName, totalSelected);
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync(); // ✅ Commit nếu không lỗi
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync(); // ❌ Rollback nếu có lỗi
                return StatusCode(500, $"Lỗi xử lý yêu cầu: {ex.Message}");
            }

            return Ok(new { message = "Yêu cầu đã được duyệt và cập nhật giao dịch thành công." });
        }


        [HttpPost("reject-royalty-request")]
        public async Task<IActionResult> RejectRoyaltyRequest([FromBody] RoyaltyRequestActionDto dto)
        {

            var request = await _context.RoyaltyPayoutRequests
                .Include(r => r.Author)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Author.UserId);
            if (request == null)
                return NotFound();

            if (request.Status != RoyaltyPayoutStatus.Pending)
                return BadRequest("Yêu cầu này đã được xử lý.");

            request.Status = RoyaltyPayoutStatus.Rejected;
            request.AdminFeedback = dto.Message;
            request.UpdateDate = DateTime.Now;
            await _mailService.SendRoyaltyRejectedMailAsync(user.Email, user.UserName, dto.Message);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Yêu cầu đã bị từ chối." });
        }

        [HttpGet("book-revenue/admin")]
        public async Task<IActionResult> GetAdminBookRevenue(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var query = _context.BookRevenueSummaries
                .Include(br => br.Book)
                    .ThenInclude(b => b.Author)
                .Where(br => br.Book != null && br.Book.Author.UserId == null);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(br => br.Book.Title.Contains(searchTerm));
            }

            var totalItems = await query.CountAsync();

            var bookRevenues = await query
                .OrderBy(br => br.Book.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = bookRevenues
                .Select((br, index) => new BookRevenueDto
                {
                    No = (pageNumber - 1) * pageSize + index + 1,
                    BookId = br.BookId,
                    BookTitle = br.Book?.Title ?? "Không rõ",
                    ImageUrl = br.Book?.ImageUrl,
                    TotalSold = br.TotalSold,
                    TotalRevenue = br.TotalRevenue
                })
                .ToList();

            var pagedResult = new PagedResult<BookRevenueDto>
            {
                Items = result,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return Ok(pagedResult);
        }

        [HttpGet("admin/book-revenue/{bookId}/transactions")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminRoyaltyTransactionsByBook(
            int bookId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // 1. Kiểm tra sách có tồn tại và là sách của admin (Author.UserId == null)
            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == bookId && b.Author != null && b.Author.UserId == null);

            if (book == null)
                return NotFound("Sách không tồn tại hoặc không phải do admin đăng.");

            // 2. Tổng số giao dịch bản quyền
            var totalItems = await _context.RoyaltyTransaction
                .Where(rt => rt.BookId == bookId)
                .CountAsync();

            // 3. Lấy danh sách giao dịch có phân trang
            var transactions = await _context.RoyaltyTransaction
                .Where(rt => rt.BookId == bookId)
                .Include(rt => rt.OrderItem)
                .OrderBy(rt => rt.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Map sang DTO
            var resultItems = transactions.Select(rt => new RoyaltyTransactionDto
            {
                Id = rt.Id,
                Amount = rt.Amount,
                IsPaid = rt.IsPaid,
                OrderId = rt.OrderItem?.OrderId
            }).ToList();

            // 5. Trả về kết quả phân trang
            var pagedResult = new PagedResult<RoyaltyTransactionDto>
            {
                Items = resultItems,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return Ok(pagedResult);
        }

        [HttpGet("admin/revenue-summary")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminRevenueSummary()
        {
            // Tổng sách
            var totalBooks = await _context.Books.CountAsync();

            // Tổng doanh thu từ sách hệ thống (không có Author)
            var systemBookRevenue = await _context.BookRevenueSummaries
                .Where(b => b.Book != null && b.Book.AuthorId == null)
                .SumAsync(b => (int?)b.TotalRevenue) ?? 0;

            // Tổng doanh thu từ sách tác giả
            var authorBookRevenue = await _context.RoyaltyTransaction
                .Where(r => r.Book != null && r.Book.AuthorId != null)
                .SumAsync(r => (int?)r.Amount) ?? 0;

            var totalRevenue = systemBookRevenue + authorBookRevenue;

            // Tổng đã trả
            var totalPaid = await _context.RoyaltyTransaction
                .Where(r => r.IsPaid)
                .SumAsync(r => (int?)r.Amount) ?? 0;

            // Tổng chưa trả
            var totalUnpaid = await _context.RoyaltyTransaction
                .Where(r => !r.IsPaid)
                .SumAsync(r => (int?)r.Amount) ?? 0;

            // Có thể thanh toán: chưa thanh toán + đơn hàng đã quá 30 ngày
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var availableToPay = await _context.RoyaltyTransaction
                .Where(rt =>
                    !rt.IsPaid &&
                    rt.OrderItem != null &&
                    rt.OrderItem.CreateDate <= thirtyDaysAgo)
                .SumAsync(rt => (int?)rt.Amount) ?? 0;

            return Ok(new
            {
                TotalRevenue = totalRevenue,
                TotalPaid = 0,
                TotalUnpaid = 0,
                Books = totalBooks,
            });
        }



    }
}
