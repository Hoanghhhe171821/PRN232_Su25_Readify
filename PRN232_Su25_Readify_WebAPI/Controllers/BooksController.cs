using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
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
        public async Task<IActionResult> AddToFavorite([FromBody] FavoriteModel model)
        {
            if (model == null || model.BookId == 0 || string.IsNullOrEmpty(model.UserId))
                return BadRequest("Invalid data");
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                model.UserId = userId;
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null) return BadRequest("Pls Login!");

            var book = await _context.Books.SingleOrDefaultAsync(b => b.Id == model.BookId);
            if (book == null) return BadRequest("Book not found");

            var isFavor = await _context.Favorite.SingleOrDefaultAsync(f => f.BookId == model.BookId && f.UserId == model.UserId);
            if (isFavor == null)
            {
                _context.Favorite.Add(new Favorite { BookId = model.BookId, UserId = model.UserId });
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
        public async Task<IActionResult> GetUserFavorites(string userId, int page = 1, int pageSize = 12,
            [FromQuery(Name = "searchBy")] string searchBy = null,
            [FromQuery(Name = "searchOption")] string searchOption = null,
            [FromQuery(Name = "cateIds")] List<int> cateIds = null,
            [FromQuery(Name = "orderBy")] string orderBy = "Desc",
            [FromQuery(Name = "isFree")] bool isFree = false)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 1) pageSize = 12;
            //Validate
            if (userId == null) return BadRequest("Pls login");
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id.Equals(userId));
            if (user == null) return BadRequest("Invalid User");

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
            var isUserExisted = await _context.Users.AnyAsync(u => u.Id.Equals(recentRead.UserId));
            if (!isUserExisted) return BadRequest("Pls login!");

            var isBookExisted = await _context.Books.AnyAsync(b => b.Id == recentRead.BookId);
            if (!isBookExisted) return BadRequest("Book not existed!");
            if (recentRead.ChapterId != null)
            {
                var isChapterExisted = await _context.Chapters.Include(c => c.Book)
                                        .AnyAsync(c => c.BookId == recentRead.BookId && c.Id == recentRead.ChapterId);
                if (!isChapterExisted) return BadRequest("Chapters not existed!");

            }

            var isExisted = await _context.RecentRead
                    .FirstOrDefaultAsync(rd => rd.UserId == recentRead.UserId && rd.BookId == recentRead.BookId);

            if (isExisted == null)
            {
                var data = new RecentRead
                {
                    BookId = recentRead.BookId,
                    UserId = recentRead.UserId,
                    ChapterId = recentRead.ChapterId,
                };
                await _context.RecentRead.AddAsync(data);

            }
            else
            {
                // Cập nhật chương mới và ngày đọc
                isExisted.ChapterId = recentRead.ChapterId;
                isExisted.DateRead = DateTime.Now;
                _context.RecentRead.Update(isExisted);
            }
            await _context.SaveChangesAsync();
            return Ok();

        }
        [HttpGet("GetAllRecentRead")]
        public async Task<IActionResult> GetAllRecentRead(string userId, int page = 1, int pageSize = 12,
            [FromQuery(Name = "searchBy")] string searchBy = null,
            [FromQuery(Name = "searchOption")] string searchOption = null,
            [FromQuery(Name = "cateIds")] List<int> cateIds = null,
            [FromQuery(Name = "orderBy")] string orderBy = "Desc",
            [FromQuery(Name = "isFree")] bool isFree = false)
        {
            // Validate user
            if (string.IsNullOrEmpty(userId)) return BadRequest("Pls login!");
            var isExistedUser = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!isExistedUser) return BadRequest("Invalid user!");

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
        // Các endpoint mới cho "Manage Books" (Contributor)
        [HttpGet("manage")]
        [Authorize(Roles = "Contributor")]
        public async Task<IActionResult> GetManageBooks(int page = 1, int pageSize = 10,
            [FromQuery(Name = "searchOption")] string searchOption = null)
        {
            var query = _context.Books.Include(b => b.Author).AsQueryable();

            if (searchOption != null) query = query.Where(b => b.Title.Contains(searchOption));

            var totalItems = await query.CountAsync();
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


        [HttpPost("manage")]
        [Authorize(Roles = "Contributor")]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            if (book == null) throw new BRException("Book data is required.");

            // Đảm bảo AuthorId và UploadedBy được gán (có thể lấy từ user hiện tại)
            var userId = User?.Identity?.Name; // Lấy ID người dùng hiện tại từ token
            if (string.IsNullOrEmpty(userId)) throw new UnauthorEx("User not authenticated.");
            book.UploadedBy = userId;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBookById), new { bookId = book.Id }, book);
        }

        [HttpPut("manage/{id}")]
        [Authorize(Roles = "Contributor")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (book == null || id != book.Id) throw new BRException("Invalid book data.");

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null) throw new NFoundEx("Book not found.");

            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.IsFree = book.IsFree;
            existingBook.Price = book.Price;
            existingBook.ImageUrl = book.ImageUrl;
            existingBook.RoyaltyRate = book.RoyaltyRate;

            _context.Entry(existingBook).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("manage/{id}")]
        [Authorize(Roles = "Contributor")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) throw new NFoundEx("Book not found.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
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
