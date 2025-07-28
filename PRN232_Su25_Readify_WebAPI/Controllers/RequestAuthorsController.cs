using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.RequestAuthor;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestAuthorsController : ControllerBase
    {
        private readonly ReadifyDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMail _mailService;
        public RequestAuthorsController(ReadifyDbContext context, UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager, IMail mailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _mailService = mailService;
        }

        [HttpGet("")]
        public async Task<IActionResult> findAll()
        {
            var requests = await _context.RequestAuthors
                .Include(r => r.User)
                .Select(r => new RequestAuthorDTO
                {
                    Id = r.Id,
                    Username = r.User.UserName,
                    FullName = r.FullName,
                    CCCD = r.CCCD,
                    BirthDate = r.BirthDate,
                    Reason = r.Reason,
                    BankAccount = r.BankAccount,
                    IsApproved = r.IsApproved,
                    CreatedDate = r.CreateDate
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("")]
        public async Task<IActionResult> Insert(PromoteAuthorRequest request)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }
            RequestAuthor requestAuthor = new RequestAuthor()
            {
                FullName = request.FullName,
                CCCD = request.CCCD,
                BirthDate = request.BirthDate,
                Reason = request.Reason,
                BankAccount = request.BankAccount,
                CreateDate = DateTime.Now,
                UserId = userId
            };
            _context.RequestAuthors.Add(requestAuthor);
            _context.SaveChanges();
            return Ok(request);
        }

        [HttpPut]
        public async Task<IActionResult> Update(RequestAuthorRequest request)
        {
            RequestAuthor requestAuthor = _context.RequestAuthors.FirstOrDefault(r => r.Id == request.Id);
            if(requestAuthor == null)
            {
                return BadRequest("Not Found");
            }

            requestAuthor.IsApproved = request.IsApproved;
            var user = await _userManager.FindByIdAsync(requestAuthor.UserId);
            List<string> userMail = new List<string>();
            userMail.Add(user.Email);
            Message messageAuthor = new Message(userMail, "Promote to be author", "Bạn đã được nâng cấp tài khoản lên thành author, vui lòng đăng nhập lại để trở thành author");
            if (user == null) throw new BRException("User is not found");
            if (requestAuthor.IsApproved == 1 )
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                var selectedRoles = currentRoles.ToList();
                selectedRoles.Add("Author");
                var resultRemove = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                var resultAdd = await _userManager.AddToRolesAsync(user, selectedRoles);
                _mailService.SendEmail(messageAuthor);
            } else
            {
                messageAuthor.Content = "Bạn không được nâng cấp tài khoản thành author do một số lý do không hợp lệ";
                _mailService.SendEmail(messageAuthor);
            }
            requestAuthor.UpdateDate = DateTime.Now;
            _context.SaveChanges();
            return Ok(requestAuthor);
        }
    }
}
