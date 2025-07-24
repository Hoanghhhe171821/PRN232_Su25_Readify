using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System;
using PRN232_Su25_Readify_WebAPI.DbContext;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ReadifyDbContext _context;
        private readonly IImageUploadService _imageUploadService;

        public UsersController(ReadifyDbContext context, IImageUploadService imageUploadService)
        {
            _context = context;
            _imageUploadService = imageUploadService;
        }

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

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Points,
                user.AvatarUrl
            });
        }
    }
}
