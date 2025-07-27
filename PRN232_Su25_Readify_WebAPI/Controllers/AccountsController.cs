using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
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
    }
}
