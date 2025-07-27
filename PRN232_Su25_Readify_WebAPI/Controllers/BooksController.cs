using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : Controller
    {
        private readonly ReadifyDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private List<Book> _books;
        public BooksController(ReadifyDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet("GetAllBooks")]
        public async Task<IActionResult> GetAllBooks(int page = 1, int pageSize = 12,
            [FromQuery(Name = "searchBy")] string searchBy = null,
            [FromQuery(Name = "searchOption")] string searchOption = null,
            [FromQuery(Name = "cateIds")] List<int> cateIds = null,
            [FromQuery(Name = "orderBy")] string orderBy = "Desc",
            [FromQuery(Name = "isFree")] bool isFree = false)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 1) pageSize = 12;

            var query = _context.Books.Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .AsQueryable();
            //Filter by isFree
            if (isFree == true) query = query.Where(b => b.IsFree == true);
            //Filter by category
            if (cateIds != null && cateIds.Any()) query = query.Where(b => b.BookCategories.Any(bc => cateIds.Contains(bc.CategoryId)));
            //Filter by Update date & Create Date
            bool isAsc = string.Equals(orderBy, "Asc", StringComparison.OrdinalIgnoreCase);
            if (isAsc)
            {
                query = query.OrderBy(b => b.UpdateDate ?? b.CreateDate);
            }
            else
            {
                query = query.OrderByDescending(b => b.UpdateDate ?? b.CreateDate);
            }
            //Filter by Search
            if (searchBy != null)
            {
                switch (searchBy)
                {
                    case "Title":
                        query = query.Where(b => b.Title.Contains(searchOption));
                        break;
                    case "Author":
                        query = query.Where(b => b.Author.Name.Contains(searchOption));
                        break;
                    default:
                        query = query.Where(b => b.Title.Contains(searchOption));
                        break;
                }
            }

            //Count
            var totalItems = await query.CountAsync();
            //Paging
            var books = await query
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            var result = new
            {
                TotalItems = totalItems,
                PageSize = pageSize,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Items = books
            };
            return Ok(result);
        }
        [HttpGet("RecommendBooks")]
        public async Task<IActionResult> GetRecommendBooks()
        {
            var books = await _context.Books
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.UnitInOrder).Take(6)
                .ToListAsync();
            return Ok(books);
        }
        [HttpGet("NewReleaseBooks")]
        public async Task<IActionResult> GetNewReleaseBooks()
        {
            var books = await _context.Books.Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.UpdateDate ?? b.CreateDate)
                .Take(6)
                .ToListAsync();
            return Ok(books);
        }
        [HttpGet("RelatedBooks/{BookId}")]
        public async Task<IActionResult> GetRelatedBooks(int BookId)
        {
            var currentBook = await _context.Books.SingleOrDefaultAsync(b => b.Id == BookId);
            if (currentBook == null) return BadRequest();

            var books = await _context.Books.Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Where(b => b.IsActive == true
                            && b.AuthorId == currentBook.AuthorId
                            && b.Id != currentBook.Id)
                .OrderByDescending(b => b.UnitInOrder).Take(3)
                .ToListAsync();
            return Ok(books);
        }
        [HttpGet("GetBookById/{bookId}")]
        public async Task<IActionResult> GetBookById(int bookId)
        {
            var books = await _context.Books.Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Include(b => b.Chapters).Include(b => b.Author)
                .Where(b => b.IsActive == true && b.Id == bookId)
                .FirstOrDefaultAsync();
            if (books == null) return NotFound();
            return Ok(books);
        }
        [HttpGet("GetAllChapterByBookId/{BookId}")]
        public async Task<IActionResult> GetAllChapterByBookId(int BookId)
        {
            var chapters = await _context.Chapters.Include(c => c.Book).Where(b => b.BookId == BookId).ToListAsync();
            if (chapters == null) return NotFound();

            return Ok(chapters);
        }

        [HttpPost("AddToFavorite")]
        public async Task<IActionResult> AddToFavorite([FromBody]FavoriteModel favoriteModel)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var book = await _context.Books.SingleOrDefaultAsync(b => b.Id == favoriteModel.BookId);
            if (book == null) return BadRequest("Book not found");

            var isFavor = await _context.Favorite.SingleOrDefaultAsync(f => f.BookId == favoriteModel.BookId && f.UserId == userId);
            if (isFavor == null)
            {
                _context.Favorite.Add(new Favorite { BookId = favoriteModel.BookId, UserId = userId });
                await _context.SaveChangesAsync();
                return Ok(new { isFavorite = true });
            }
            else
            {
                _context.Favorite.Remove(isFavor);
                await _context.SaveChangesAsync();
                return Ok(new { isFavorite = false });
            }
        }
        [HttpGet("GetUserFavorites")]
        public async Task<IActionResult> GetUserFavorites( int page = 1, int pageSize = 12,
            [FromQuery(Name = "searchBy")] string searchBy = null,
            [FromQuery(Name = "searchOption")] string searchOption = null,
            [FromQuery(Name = "cateIds")] List<int> cateIds = null,
            [FromQuery(Name = "orderBy")] string orderBy = "Desc",
            [FromQuery(Name = "isFree")] bool isFree = false)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 1) pageSize = 12;
            //Validate
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            //Get List Favor Book Id
            var favorBookIds = await _context.Favorite.Where(f => f.UserId.Equals(userId)).Select(f => f.BookId).ToListAsync();

            var query = _context.Books.Include(b => b.BookCategories).ThenInclude(bc => bc.Category).Where(b => favorBookIds.Contains(b.Id))
                .AsQueryable();

            //Filter by isFree
            if (isFree == true) query = query.Where(b => b.IsFree == true);
            //Filter by category
            if (cateIds != null && cateIds.Any()) query = query.Where(b => b.BookCategories.Any(bc => cateIds.Contains(bc.CategoryId)));
            //Filter by Update date & Create Date
            bool isAsc = string.Equals(orderBy, "Asc", StringComparison.OrdinalIgnoreCase);
            if (isAsc)
            {
                query = query.OrderBy(b => b.UpdateDate ?? b.CreateDate);
            }
            else
            {
                query = query.OrderByDescending(b => b.UpdateDate ?? b.CreateDate);
            }
            //Filter by Search
            if (searchBy != null)
            {
                switch (searchBy)
                {
                    case "Title":
                        query = query.Where(b => b.Title.Contains(searchOption));
                        break;
                    case "Author":
                        query = query.Where(b => b.Author.Name.Contains(searchOption));
                        break;
                    default:
                        query = query.Where(b => b.Title.Contains(searchOption));
                        break;
                }
            }

            //Count
            var totalItems = await query.CountAsync();
            //Paging
            var books = await query
    .Skip((page - 1) * pageSize).Take(pageSize)
    .ToListAsync();
            var result = new
            {
                TotalItems = totalItems,
                PageSize = pageSize,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Items = books
            };
            return Ok(result);
        }

        [HttpPost("AddToRecentRead")]
        public async Task<IActionResult> AddToRecentRead([FromBody] RecentReadModel recentRead)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var isBookExisted = await _context.Books.AnyAsync(b => b.Id == recentRead.BookId);
            if (!isBookExisted) return BadRequest("Book not existed!");

            if (recentRead.ChapterId != null)
            {
                var isChapterExisted = await _context.Chapters.Include(c => c.Book)
                                        .AnyAsync(c => c.BookId == recentRead.BookId && c.Id == recentRead.ChapterId);
                if (!isChapterExisted) return BadRequest("Chapters not existed!");

            }

            var existing = await _context.RecentRead
                                    .FirstOrDefaultAsync(rd =>
                                        rd.UserId == userId &&
                                        rd.BookId == recentRead.BookId &&
                                        rd.ChapterId == recentRead.ChapterId);
            if (existing == null)
            {
                var data = new RecentRead
                {
                    BookId = recentRead.BookId,
                    UserId = userId,
                    ChapterId = recentRead.ChapterId,
                    DateRead = DateTime.Now
                };
                await _context.RecentRead.AddAsync(data);
            }
            else
            {
                existing.DateRead = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return Ok();

        }
        [HttpGet("GetAllRecentRead")]
        public async Task<IActionResult> GetAllRecentRead( int page = 1, int pageSize = 12,
            [FromQuery(Name = "searchBy")] string searchBy = null,
            [FromQuery(Name = "searchOption")] string searchOption = null,
            [FromQuery(Name = "cateIds")] List<int> cateIds = null,
            [FromQuery(Name = "orderBy")] string orderBy = "Desc",
            [FromQuery(Name = "isFree")] bool isFree = false)
        {
            // Validate user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            // Validate paging
            if (page <= 0) page = 1;
            if (pageSize <= 1) pageSize = 12;

            // Get list of recently read book IDs
            var recentReadIds = await _context.RecentRead
                .Where(rd => rd.UserId == userId)
                .Select(rd => rd.BookId)
                .ToListAsync();

            // Query books
            var query = _context.Books
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Where(b => recentReadIds.Contains(b.Id))
                .AsQueryable();

            // Filter by isFree
            if (isFree) query = query.Where(b => b.IsFree);

            // Filter by category
            if (cateIds != null && cateIds.Any())
                query = query.Where(b => b.BookCategories.Any(bc => cateIds.Contains(bc.CategoryId)));

            // Filter by searchOption
            if (searchBy != null)
            {
                switch (searchBy)
                {
                    case "Title":
                        query = query.Where(b => b.Title.Contains(searchOption));
                        break;
                    case "Author":
                        query = query.Where(b => b.Author.Name.Contains(searchOption));
                        break;
                    default:
                        query = query.Where(b => b.Title.Contains(searchOption));
                        break;
                }
            }
            // Sort
            bool isAsc = string.Equals(orderBy, "Asc", StringComparison.OrdinalIgnoreCase);
            query = isAsc
                ? query.OrderBy(b => b.UpdateDate ?? b.CreateDate)
                : query.OrderByDescending(b => b.UpdateDate ?? b.CreateDate);

            // Count & paging
            var totalItems = await query.CountAsync();
            var books = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new
            {
                TotalItems = totalItems,
                PageSize = pageSize,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Items = books
            };

            return Ok(result);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Author")]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto dto)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            //var author = await _context.Authors.FindAsync(dto.AuthorId);
            //if (author == null)
            //    return BadRequest("Author not found");

            ////if (searchOption != null) query = query.Where(b => b.Title.Contains(searchOption));

            //// Lấy ID người dùng hiện tại từ token
            //var userId = User?.Identity?.Name;
            //if (string.IsNullOrEmpty(userId)) throw new UnauthorEx("User not authenticated.");

            //var book = new Book
            //{
            //    Title = dto.Title,
            //    Description = dto.Description,
            //    IsFree = dto.IsFree,
            //    Price = dto.Price,
            //    UnitInOrder = dto.UnitInOrder,
            //    ImageUrl = dto.ImageUrl,
            //    RoyaltyRate = dto.RoyaltyRate,
            //    AuthorId = dto.AuthorId,
            //    UploadedBy = dto.UploadedBy,
            //    CreateDate = dto.CreateDate,
            //    UpdateDate = dto.UpdateDate,
            //    IsActive = false // mặc định chưa duyệt
            //};

            //_context.Books.Add(book);
            //await _context.SaveChangesAsync();

            //return Ok(new
            //{
            //    message = "Book uploaded successfully, pending approval",
            //    bookId = book.Id
            //});
            return Ok();
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingBooks()
        {
            var pendingBooks = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.IsActive == false)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Description,
                    b.CreateDate,
                    AuthorName = b.Author.User.UserName,
                    b.RoyaltyRate
                })
                .ToListAsync();

            return Ok(pendingBooks);
        }

        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Book not found");

            book.IsActive = true;
            book.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Book approved and now visible." });
        }

        [HttpPut("UpdateBook")]
        [Authorize(Roles = "Contributor")]
        public async Task<IActionResult> UpdateBook([FromBody] UpdateBookDto updateBookDto)
        {
            //var book = await _context.Books.FindAsync(updateBookDto.Id);
            //if (book == null) return NotFound("Book not found");

            //// Gán giá trị cập nhật từ DTO sang entity
            //book.Title = updateBookDto.Title;
            //book.Description = updateBookDto.Description;
            //book.IsFree = updateBookDto.IsFree;
            //book.Price = updateBookDto.Price;
            //book.UnitInOrder = updateBookDto.UnitInOrder;
            //book.ImageUrl = updateBookDto.ImageUrl;
            //book.RoyaltyRate = updateBookDto.RoyaltyRate;
            //book.AuthorId = updateBookDto.AuthorId;
            //book.UpdateDate = DateTime.Now;

            //await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("ToggleBookStatus/{id}")]
        [Authorize(Roles = "Contributor")]
        public async Task<IActionResult> ToggleBookStatus(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Book not found");

            // Đảo trạng thái: nếu đang active → inactive, và ngược lại
            book.IsActive = !book.IsActive;
            book.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            string status = book.IsActive ? "activated" : "deactivated";
            return Ok(new { message = $"Book has been {status}.", isActive = book.IsActive });
        }


        [HttpGet("checkBookLicence/{bookId}")]
        public async Task<ActionResult<bool>> GetBookLicense(int bookId)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Bạn cần đăng nhập.");

            var isBookLicensed = await _context.BookLicenses
                .AnyAsync(bl => bl.BookId == bookId && bl.UserId == userId);

            return Ok(isBookLicensed);
        }

    }
}