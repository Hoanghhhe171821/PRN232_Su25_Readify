using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Reports;
using PRN232_Su25_Readify_WebAPI.Models.Enum;
using PRN232_Su25_Readify_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookReportsController : ControllerBase
    {
        private readonly ReadifyDbContext _context;

        public BookReportsController(ReadifyDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReportBook([FromBody] CreateBookReportDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var report = new BookReport
            {
                BookId = dto.BookId,
                ReporterId = userId,
                Reason = dto.Reason,
                Description = dto.Description
            };
            _context.BookReports.Add(report);
            await _context.SaveChangesAsync();
            return Ok("Báo cáo đã được gửi.");
        }

        //[Authorize(Roles = "Manager")]
        [HttpPut("handle")]
        public async Task<IActionResult> HandleReport([FromBody] HandleReportDto dto)
        {
            var report = await _context.BookReports.Include(r => r.Book).FirstOrDefaultAsync(r => r.Id == dto.ReportId);
            if (report == null) return NotFound();

            switch (dto.Action.ToLower())
            {
                case "warning":
                    report.Status = BookReportStatus.WARNING;
                    break;
                case "lock":
                    report.Status = BookReportStatus.BOOK_LOCKED;
                    report.Book.IsActive = false;
                    break;
                case "resolve":
                    report.Status = BookReportStatus.RESOLVED_NO_ACTION;
                    break;
                default:
                    return BadRequest("Action không hợp lệ.");
            }

            report.ManagerNote = dto.Note;
            await _context.SaveChangesAsync();
            return Ok("Đã xử lý báo cáo.");
        }

        //[Authorize(Roles = "Author")]
        [HttpPut("author-response")]
        public async Task<IActionResult> AuthorResponse([FromBody] AuthorResponseDto dto)
        {
            var report = await _context.BookReports
                .Include(r => r.Book)
                .Include(r => r.Book.Author)
                .FirstOrDefaultAsync(r => r.Id == dto.ReportId);

            if (report == null || report.Status != BookReportStatus.WARNING)
                return NotFound();

            if (report.Book.Author.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            if (dto.AcceptLock)
            {
                report.Status = BookReportStatus.BOOK_LOCKED;
                report.Book.IsActive = false; // 🔒 KHÓA SÁCH
            }
            else
            {
                report.Status = BookReportStatus.AUTHOR_DENIED_LOCK;
                report.AuthorResponse = dto.Reason;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }


        //[Authorize(Roles = "Manager")]
        [HttpPut("manager-decision")]
        public async Task<IActionResult> HandleAuthorDenial([FromBody] ManagerDecisionDto dto)
        {
            var report = await _context.BookReports.Include(r => r.Book).FirstOrDefaultAsync(r => r.Id == dto.ReportId);
            if (report == null) return NotFound();

            if (dto.AcceptAuthorDenial)
            {
                report.Status = BookReportStatus.CLOSED_ACCEPTED_DENIAL;
                report.Book.IsActive = true; // nếu đã khóa thì mở lại
            }
            else
            {
                report.Status = BookReportStatus.CLOSED_REJECTED_DENIAL;
                report.Book.IsActive = false; // KHÓA sách
            }

            report.ManagerNote = dto.Note;
            await _context.SaveChangesAsync();
            return Ok();
        }

        //[Authorize(Roles = "Manager")]
        [HttpGet("/api/Reports/manager")]
        public async Task<IActionResult> GetReportsForManager()
        {
            var reports = await _context.BookReports
                .Include(r => r.Book)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var result = reports.Select(r => new
            {
                ReportId = r.Id,
                BookTitle = r.Book.Title,
                ReporterName = r.Reporter.UserName,
                Reason = r.Reason,
                Status = r.Status.ToString()
            });

            return Ok(result);
        }
        //[Authorize(Roles = "Manager")]
        [HttpGet("/api/Reports/{id}/detail")]
        public async Task<IActionResult> GetReportDetail(int id)
        {
            var report = await _context.BookReports
                .Include(r => r.Book)
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            var result = new
            {
                ReportId = report.Id,
                BookTitle = report.Book.Title,
                BookId = report.Book.Id,
                ReporterName = report.Reporter.UserName,
                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status.ToString(),
                ManagerNote = report.ManagerNote,
                AuthorDenialReason = report.AuthorResponse  
            };

            return Ok(result);
        }
        //[Authorize(Roles = "Author")]
        [HttpGet("warning/{id}")]
        public async Task<IActionResult> GetWarningReport(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var report = await _context.BookReports
                .Include(r => r.Book)
                .Include(r => r.Reporter)
                .Include(r => r.Book.Author)
                .FirstOrDefaultAsync(r => r.Id == id && r.Book.Author.UserId == userId);

            if (report == null || report.Status != BookReportStatus.WARNING)
                return NotFound();

            var result = new
            {
                ReportId = report.Id,
                BookTitle = report.Book.Title,
                ReporterName = report.Reporter.UserName,
                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status.ToString(),
                ManagerNote = report.ManagerNote
            };

            return Ok(result);
        }

        //[Authorize(Roles = "Author")]
        [HttpGet("pending-warnings")]
        public async Task<IActionResult> GetPendingWarnings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var warnings = await _context.BookReports
                .Include(r => r.Book)
                .Where(r => r.Book.Author.UserId == userId && r.Status == BookReportStatus.WARNING)
                .Select(r => new
                {
                    r.Id,
                    BookTitle = r.Book.Title,
                    r.Reason,
                    r.Description,
                    r.Status
                })
                .ToListAsync();

            return Ok(warnings);
        }
        [HttpGet("{id}/author-denied")]
        public async Task<IActionResult> GetAuthorDeniedReport(int id)
        {
            var report = await _context.BookReports.Include(r => r.Book)
                                                   .FirstOrDefaultAsync(r => r.Id == id && r.Status == BookReportStatus.AUTHOR_DENIED_LOCK);

            if (report == null) return NotFound();

            return Ok(new
            {
                ReportId = report.Id,
                BookTitle = report.Book.Title,
                AuthorResponse = report.AuthorResponse
            });
        }

    }

}
