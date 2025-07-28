using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models.Common;
using Newtonsoft.Json;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Text.RegularExpressions;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerBooksController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly ReadifyDbContext _context;

        public ManagerBooksController(ReadifyDbContext context, IImageUploadService imageUploadService)
        {
            _context = context;
            _imageUploadService = imageUploadService;
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
                TotalItems = totalItems
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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBooks(int page = 1)
        {
            const int pageSize = 5;

            var query = _context.Books
                .Include(b => b.Author)
                    .ThenInclude(a => a.User)
                .OrderByDescending(b => b.CreateDate)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new ManagerBookListItemViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    ImageUrl = b.ImageUrl,
                    AuthorName = b.Author.User.UserName,
                    CreateDate = (DateTime)b.CreateDate,
                    IsActive = b.IsActive,
                    IsPublished = b.IsPublished
                })
                .ToListAsync();

            var response = new PaginatedResponse<ManagerBookListItemViewModel>
            {
                Items = books,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return Ok(response);
        }

 
        [HttpGet("GetBookDetailsById/{bookId}")]
        public async Task<IActionResult> GetBookDetailsById(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

          

            var book = await _context.Books.Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Include(b => b.Chapters).Include(b => b.Author)
                .Where(b =>  b.Id == bookId)
                .FirstOrDefaultAsync();
            if (book == null) return NotFound();
            return Ok(book);

        }

 
        [HttpPost("CreateBook")]
        public async Task<IActionResult> CreateBook([FromForm] CreateBookWithFile request)
        {
            if (request.ImageFile == null || request.ImageFile.Length == 0)
                return BadRequest("No file uploaded.");

            if (request.ImageFile.Length > 5 * 1024 * 1024)
                return BadRequest("File too large.");

            var extension = Path.GetExtension(request.ImageFile.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                return BadRequest("Invalid image format. Only JPG/PNG allowed.");

            //Validate
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var author = _context.Authors.FirstOrDefault(b => b.UserId == userId);
            if (author == null) return Unauthorized();

            var model = JsonConvert.DeserializeObject<CreateBookDto>(request.BookData);
            if (model == null) return BadRequest("Invalid book data");

            // Generate file path
            var safeTitleForFileName = Slugify(model.Title);
            var imageUrl = await _imageUploadService.UploadImageAsync(request.ImageFile, "book-covers", safeTitleForFileName);

            Book newBook = new Book
            {
                Title = model.Title,
                Description = model.Description,
                IsFree = model.IsFree,
                Price = model.Price,
                ImageUrl = imageUrl,
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

        [HttpPut("UpdateBook")]
        public async Task<IActionResult> UpdateBook([FromBody] CreateBookWithFile request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var author = _context.Authors.FirstOrDefault(b => b.UserId == userId);
            if (author == null) return Unauthorized();

            var model = JsonConvert.DeserializeObject<UpdateBookDto>(request.BookData);
            if (model == null) return BadRequest("Invalid book data");

            var book = await _context.Books
                            .Include(b => b.BookCategories)
                            .FirstOrDefaultAsync(b => b.Id == model.Id && b.AuthorId == author.Id && b.IsPublished == true);

            if (book == null)
                return NotFound(new { message = "Không tìm thấy sách" });

            book.Title = model.Title;
            book.Description = model.Description;
            book.IsFree = model.IsFree;
            book.Price = model.Price;
            book.UpdateDate = DateTime.UtcNow;

            // Nếu có ảnh mới thì upload
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var safeTitleForFileName = Slugify(model.Title);
                var newImageUrl = await _imageUploadService.UploadImageAsync(request.ImageFile, "book-covers", safeTitleForFileName);
                book.ImageUrl = newImageUrl;
            }

            // Cập nhật category
            _context.BookCategories.RemoveRange(book.BookCategories);

            if (model.CategoryIds != null && model.CategoryIds.Any())
            {
                foreach (var cateId in model.CategoryIds)
                {
                    _context.BookCategories.Add(new BookCategory
                    {
                        BookId = book.Id,
                        CategoryId = cateId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(book);
        }

        public static string Slugify(string phrase)
        {
            string str = phrase.ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");  // Bỏ ký tự đặc biệt
            str = Regex.Replace(str, @"\s+", " ").Trim();   // Rút gọn khoảng trắng
            str = Regex.Replace(str, @"\s", "-");           // Đổi space thành -
            return str;
        }

    }
}
