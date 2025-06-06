using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : Controller
    {
        private readonly Prn232Su25FinalProjectReadifyContext _context;
        private List<Book> _books;
        public BooksController(Prn232Su25FinalProjectReadifyContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _context.Books.Include(b => b.Author).Include(b => b.Chapters).
                ToListAsync();
            if (!books.Any()) return NotFound();
            return Ok(books);
        }
        [HttpGet("{BookId}")]
        public async Task<IActionResult> GetAllChapterByBookId(int BookId)
        {
            var chapters = _context.Chapters.Include(c => c.Book).Where( b => b.BookId == BookId ).ToListAsync();
            if(chapters == null) return NotFound();

            return Ok(chapters);
        }


    }
}
