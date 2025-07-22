using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;

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
        [HttpGet("GetAllAuthors")]
        public async Task<IActionResult> GetAllAuthors()
        {
            var author = await _context.Authors.ToListAsync();
            return Ok(author);
        }
    }
}
