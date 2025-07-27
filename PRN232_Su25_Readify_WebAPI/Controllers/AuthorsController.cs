using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Authors;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Dtos.RoyaltyPayout;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ReadifyDbContext _context;
        private readonly IRoyalPayoutReService _royalPayoutReService;

        public AuthorsController(ReadifyDbContext context, IRoyalPayoutReService royalPayoutReService)
        {
            _context = context;
            _royalPayoutReService = royalPayoutReService; 
        }
        [HttpGet("GetAllAuthors")]
        public async Task<IActionResult> GetAllAuthors()
        {
            var author = await _context.Authors.ToListAsync();
            return Ok(author);
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

        [HttpGet("RequestPublishBook")]
        public async Task<IActionResult> GetRequestPublishBook()
        {
            var books = await _context.Books.Where(b => b.IsPublished == false).ToListAsync();
            return Ok(books);
        }

        [Authorize(Roles = "Author")]
        [HttpGet("GetAllBooksByAuthor")]
        public async Task<IActionResult> GetAllBooksByAuthor()
        {
            //Validate
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var author = _context.Authors.FirstOrDefault(b => b.UserId == userId);
            if (author == null) return Unauthorized();

            var books = await _context.Books.Where(a => a.AuthorId == author.Id)
                                    .ToListAsync();
            return Ok(books);
        }

        [Authorize(Roles = "Author")]
        [HttpPost("CreateBook")]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto model)
        {
            //Validate
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var author = _context.Authors.FirstOrDefault(b => b.UserId == userId);
            if (author == null) return Unauthorized();

            Book newBook = new Book
            {
                Title = model.Title,
                Description = model.Description,
                IsFree = model.IsFree,
                Price = model.Price,
                ImageUrl = model.ImageUrl,
                RoyaltyRate = model.RoyaltyRate,
                AuthorId = author.Id,
                UploadedBy = userId,
                IsPublished = model.IsPublished,
                CreateDate = DateTime.UtcNow,
                IsActive = true

            };

            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();

            if (model.CategoryIds != null && model.CategoryIds.Any())
            {
                foreach (var cateId in model.CategoryIds)
                {
                    _context.BookCategories.Add(new BookCategory
                    {
                        BookId = newBook.Id,
                        CategoryId = cateId
                    });
                }

                await _context.SaveChangesAsync();
            }

            return Ok(newBook);
        }

    }
}
