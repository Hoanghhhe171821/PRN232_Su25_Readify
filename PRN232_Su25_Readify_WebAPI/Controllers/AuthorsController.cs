using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Authors;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ReadifyDbContext _context;

        public AuthorsController(ReadifyDbContext context)
        {
            _context = context;
        }
        // Lấy thông tin công khai của 1 tác giả bất kỳ
        [HttpGet("{authorId}/profile")]
        public async Task<IActionResult> GetAuthorProfile(int authorId)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == authorId);

            if (author == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dto = new AuthorProfileDto
            {
                Name = author.Name,
                Bio = author.Bio,
                AvatarUrl = author.User?.AvatarUrl,
                Books = author.Books.Select(b => new BookSummary
                {
                    Id = b.Id,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl
                }).ToList(),
                ContactEmail = author.PublicEmail,
                ContactPhone = author.PublicPhone
            };

            return Ok(dto);
        }
    }
}
