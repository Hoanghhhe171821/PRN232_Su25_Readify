using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : Controller
    {
        private readonly ReadifyDbContext _context;
        private List<Book> _books;
        public BooksController(ReadifyDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetAllFreeBooks")]
        public async Task<IActionResult> GetAllFreeBooks()
        {
            var books = await _context.Books.Where(b => b.IsFree == true).
                ToListAsync();
            if (!books.Any()) return NotFound();
            return Ok(books);
        }
        [HttpGet("GetAllBooks")]
        public async Task<IActionResult> GetAllBooks(int page = 1, int pageSize = 12,
            [FromQuery(Name = "searchTitle")] string searchTitle = null,
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
            //Filter by Search Title
            if (searchTitle != null) query = query.Where(b => b.Title.Contains(searchTitle));


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
            [FromQuery(Name = "searchTitle")] string searchTitle = null,
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
            //Filter by Search Title
            if (searchTitle != null) query = query.Where(b => b.Title.Contains(searchTitle));


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

    }
}
