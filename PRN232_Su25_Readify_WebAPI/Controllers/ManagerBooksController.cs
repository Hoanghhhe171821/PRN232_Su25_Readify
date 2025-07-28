using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models.Common;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerBooksController : ControllerBase
    {
        private readonly ReadifyDbContext _context;

        public ManagerBooksController(ReadifyDbContext context)
        {
            _context = context;
        }

        // GET: api/ManagerBooks?page=1&search=abc
        [HttpGet]
        public async Task<IActionResult> GetPendingBooks(int page = 1, string search = "")
        {
            const int pageSize = 10;

            var query = _context.Books
                .Include(b => b.Author)
                .ThenInclude(a => a.User)
                .Where(b => !b.IsPublished && b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.Trim().ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(lowerSearch) ||
                    b.Author.User.UserName.ToLower().Contains(lowerSearch));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var books = await query
                .OrderByDescending(b => b.CreateDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new ManagerBookReviewViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    ImageUrl = b.ImageUrl,
                    AuthorName = b.Author.User.UserName,
                    CreateDate = (DateTime)b.CreateDate,
                    IsActive = b.IsActive
                })
                .ToListAsync();

            var response = new PaginatedResponse<ManagerBookReviewViewModel>
            {
                Items = books,
                Page = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                SearchKeyword = search
            };

            return Ok(response);
        }

        // PUT: api/ManagerBooks/decision
        [HttpPut("decision")]
        public async Task<IActionResult> ApproveOrReject([FromBody] ManagerBookDecisionDto dto)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == dto.BookId);
            if (book == null) return NotFound("Không tìm thấy sách.");

            book.IsPublished = dto.IsApproved;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = dto.IsApproved ? "Sách đã được duyệt." : "Sách đã bị từ chối."
            });
        }
    }
}
