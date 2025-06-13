using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.DbContext;
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
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _context.Books
                .ToListAsync();
            if (!books.Any()) return NotFound();
            return Ok(books);
        }
        [HttpGet("RecommendBooks")]
        public async Task<IActionResult> GetRecommendBooks()
        {
            var books = await _context.Books
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.UnitInOrder).Take(4)
                .ToListAsync();
            if (!books.Any()) return NotFound();
            return Ok(books);
        }
        [HttpGet("NewReleaseBooks")]
        public async Task<IActionResult> GetNewReleaseBooks()
        {
            var books = await _context.Books.Include(b =>  b.BookCategories).ThenInclude(bc => bc.Category)
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.UpdateDate ?? b.CreateDate)
                .Take(6)
                .ToListAsync();
            if (!books.Any()) return NotFound();
            return Ok(books);
        }
        [HttpGet("GetBookById/{BookId}")]
        public async Task<IActionResult> GetBookById(int bookId)
        {
            var books = await _context.Books.Include( b=> b.Chapters).Include(b => b.Author)
                .Where(b => b.IsActive == true && b.Id == bookId)
                .ToListAsync();
            if (!books.Any()) return NotFound();
            return Ok(books);
        }
        [HttpGet("GetAllChapterByBookId/{BookId}")]
        public async Task<IActionResult> GetAllChapterByBookId(int BookId)
        {
            var chapters = await _context.Chapters.Include(c => c.Book).Where( b => b.BookId == BookId ).ToListAsync();
            if(chapters == null) return NotFound();

            return Ok(chapters);
        }
        [HttpPost("{BookId}")]
        public async Task<IActionResult> UpdateBookByBookId(int BookId, [FromBody] Book updateBook)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == BookId);

            if (book == null) return NotFound();

            //book.Title = updateBook.Title;
            //book.Description = updateBook.Description;
            //book.IsFree = updateBook.IsFree;
            //book.Price = updateBook.Price;
            //book.Status = updateBook.Status;

            await _context.SaveChangesAsync();
            

            return Ok(book);
        }


    }
}
