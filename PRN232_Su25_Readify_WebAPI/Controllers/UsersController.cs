using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Users;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ReadifyDbContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly UserManager<AppUser> _userManager;

        public UsersController(ReadifyDbContext context, IImageUploadService imageUploadService, UserManager<AppUser> userManager)
        {
            _context = context;
            _imageUploadService = imageUploadService;
            _userManager = userManager;
        }

        // ✅ GET profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserId == userId);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Points,
                user.AvatarUrl,
                IsAuthor = author != null,
                Bio = author?.Bio,
                PublicEmail = author?.PublicEmail,
                PublicPhone = author?.PublicPhone
            });
        }

        // ✅ POST avatar only
        [Authorize]
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null)
                return BadRequest("No file uploaded");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var avatarUrl = await _imageUploadService.UploadImageAsync(file, "avatars", $"user_{user.Id}");
            if (avatarUrl == null)
                return StatusCode(500, "Upload failed");

            user.AvatarUrl = avatarUrl;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.UserName, AvatarUrl = user.AvatarUrl });
        }

        // ✅ PUT update profile (username, phone, avatar)
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            if (!string.IsNullOrWhiteSpace(dto.UserName)) user.UserName = dto.UserName;
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;

            // Avatar upload
            if (dto.Avatar != null)
            {
                var avatarUrl = await _imageUploadService.UploadImageAsync(dto.Avatar, "avatars", $"user_{user.Id}");
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    user.AvatarUrl = avatarUrl;
                }
            }

            // Nếu user là tác giả thì cập nhật thêm
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.UserId == userId);
            if (author != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Bio)) author.Bio = dto.Bio;
                if (!string.IsNullOrWhiteSpace(dto.PublicEmail)) author.PublicEmail = dto.PublicEmail;
                if (!string.IsNullOrWhiteSpace(dto.PublicPhone)) author.PublicPhone = dto.PublicPhone;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated",
                user.UserName,
                user.PhoneNumber,
                user.AvatarUrl,
                Bio = author?.Bio,
                PublicEmail = author?.PublicEmail,
                PublicPhone = author?.PublicPhone
            });
        }

        // ✅ PUT change password
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Confirmation password does not match");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new { message = "Password changed successfully" });
        }
    }
}
