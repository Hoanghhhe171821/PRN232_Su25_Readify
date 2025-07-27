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
    }
}
