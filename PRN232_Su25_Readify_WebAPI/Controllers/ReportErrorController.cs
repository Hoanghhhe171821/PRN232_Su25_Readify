using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Chapters;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Models.Enum;
using System;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportErrorController : ControllerBase
    {
        private readonly ReadifyDbContext _context;

        public ReportErrorController(ReadifyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ReportChapterError([FromBody] ChapterErrorReportDto dto)
        {
            // Kiểm tra chapter tồn tại
            var chapter = await _context.Chapters.FindAsync(dto.ChapterId);
            if (chapter == null)
                return NotFound("Chapter not found");

            // Kiểm tra error type hợp lệ
            var errorType = await _context.ErrorTypes.FindAsync(dto.ErrorTypeId);
            if (errorType == null)
                return BadRequest("Invalid error type");

            // Lấy UserId từ token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Tạo đối tượng ChapterError
            var error = new ChapterError
            {
                ChapterId = dto.ChapterId,
                ErrorTypeId = dto.ErrorTypeId,
                Description = dto.Description,
                Status = ErrorStatus.Pending,
                UserId = userId
            };

            _context.ChaptersError.Add(error);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Error reported successfully", errorId = error.Id });
        }
        // API: /api/ReportError/GetAllReports
        [HttpGet("GetAllReports")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.ChaptersError
                .Include(e => e.Chapter)
                .Include(e => e.ErrorType)
                .Include(e => e.User)
                .OrderByDescending(e => e.Id)
                .Select(e => new
                {
                    e.Id,
                    e.Description,
                    e.Status,
                    ChapterTitle = e.Chapter.Title,
                    ErrorTypeName = e.ErrorType.Name,
                    UserEmail = e.User.Email
                })
                .ToListAsync();

            return Ok(reports);
        }
    }
}
