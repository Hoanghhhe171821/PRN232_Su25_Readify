using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Authors;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Dtos.RoyaltyPayout;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;
using System.Numerics;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ReadifyDbContext _context;
        private readonly IRoyalPayoutReService _royalPayoutReService;
        private readonly IImageUploadService _imageUploadService;
        public AuthorsController(ReadifyDbContext context, IRoyalPayoutReService royalPayoutReService, IImageUploadService imageUploadService)
        {
            _context = context;
            _royalPayoutReService = royalPayoutReService;
            _imageUploadService = imageUploadService;   
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
        public async Task<IActionResult> GetAllBooksByAuthor(int pageNumber = 1)
        {
            const int pageSize = 7;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var author = await _context.Authors.FirstOrDefaultAsync(b => b.UserId == userId);
            if (author == null) return Unauthorized();

            var totalBooks = await _context.Books.CountAsync(b => b.AuthorId == author.Id);
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var books = await _context.Books
                .Where(b => b.AuthorId == author.Id)
                .OrderByDescending(b => b.UpdateDate ?? b.CreateDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalBooks = totalBooks,
                Books = books
            });
        }

        [Authorize(Roles = "Author")]
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

        [Authorize(Roles = "Author")]
        [HttpPut("UpdateBook")]
        public async Task<IActionResult> UpdateBook([FromBody] UpdateBookDto model)
        {
            //Validate
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var author = _context.Authors.FirstOrDefault(b => b.UserId == userId);
            if (author == null) return Unauthorized();


            var book = await _context.Books
                            .Include(b => b.BookCategories)
                            .FirstOrDefaultAsync(b => b.Id == model.Id && b.AuthorId == author.Id && b.IsPublished == true);

            if (book == null)
                return NotFound(new { message = "Không tìm thấy sách" });

            book.Title = model.Title;
            book.Description = model.Description;
            book.IsFree = model.IsFree;
            book.Price = model.Price;
            book.ImageUrl = model.ImageUrl;
            book.UpdateDate = DateTime.UtcNow;
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

        [HttpGet("revenue-summary")]
        public async Task<IActionResult> GetAuthorRevenueSummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated.");

            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (author == null) return NotFound("Author not found.");
            var summarry = await _context.AuthorRevenueSummary
                .Where(s => s.AuthorId == author.Id)
                .FirstOrDefaultAsync();
            if(summarry == null)
            {
                summarry = new AuthorRevenueSummary
                {
                    TotalRevenue = 0,
                    TotalPaid = 0
                };
            }
            var listBooks = _context.Books.Where(b => b.AuthorId == author.Id).Count();
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var totalAvailRevenue = await _context.RoyaltyTransaction
                .Where(rt =>
                    rt.AuthorId == author.Id &&
                    rt.IsPaid == false &&
                    rt.OrderItem != null &&
                    rt.OrderItem.CreateDate <= thirtyDaysAgo
                )
                .SumAsync(rt => (int?)rt.Amount) ?? 0;


            return Ok(new
            {
                TotalRevenue = summarry.TotalRevenue,
                TotalPaid = summarry.TotalPaid,
                TotalUnpaid = summarry.TotalUnpaid,
                Books = listBooks,
                AvailRevenue = totalAvailRevenue
            });
        }


        [HttpGet("royal-transaction-author")]
        public async Task<IActionResult> GetRoyalTransaction()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.UserId == userId);
            if (author == null)
                return NotFound("Author not found.");

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var totalRevenue = await _context.RoyaltyTransaction
                .Where(rt =>
                    rt.AuthorId == author.Id &&
                    rt.IsPaid == false &&
                    rt.OrderItem != null &&
                    rt.OrderItem.CreateDate <= thirtyDaysAgo
                )
                .SumAsync(rt => (int?)rt.Amount) ?? 0;

            return Ok(new
            {
                TotalRevenueUnpaidLast30Days = totalRevenue
            });
        }

        [HttpGet("revenue-books")]
        public async Task<IActionResult> GetRevenueBooks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.UserId == userId);
            if (author == null)
                return NotFound("Author not found.");

            var listBook = await _context.Books.Where(b => b.AuthorId == author.Id).Select(lb => lb.Id).ToListAsync();
            var listBookRevenue = await _context.BookRevenueSummaries
                                    .Where(brv => listBook.Contains(brv.BookId)).ToListAsync();

            return Ok(listBookRevenue);
        }

        //[Authorize(Roles = "Author")]
        [HttpPost("royal-payout-request")]
        public async Task<IActionResult> CreateRequest([FromBody] int request)
        {
            try
            {
                var created = await _royalPayoutReService.CreateRequestAsync(request);
                return Ok(created);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Bạn không có quyền tạo yêu cầu.");
            }
        }

        //[Authorize(Roles = "Author")]
        [HttpGet("author-request-pay")]
        public async Task<IActionResult> GetByAuthor(int page = 1, int pageSize = 5)
        {
            try
            {
                var result = await _royalPayoutReService.GetRequestsByAuthorIdAsync(page, pageSize);

                // Map dữ liệu tinh gọn
                var simplifiedItems = result.Items.Select(x => new RoyaltyRequestDto
                {
                    Id = x.Id,
                    RequestAmount = x.RequestAmount,
                    ApprovedAmount = x.ApprovedAmount,
                    Status = x.Status.ToString(),
                    CreateDate = x.CreateDate ?? DateTime.Now
                }).ToList();

                // Trả kết quả phân trang tinh gọn
                var response = new
                {
                    items = simplifiedItems,
                    currentPage = result.CurrentPage,
                    pageSize = result.PageSize,
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Không thể xác định tác giả từ token.");
            }
        }


        //[Authorize(Roles = "Admin")]
        [HttpGet("royal-request-all")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            var result = await _royalPayoutReService.GetAllRequestsAsync(page, pageSize);
            return Ok(result);
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
