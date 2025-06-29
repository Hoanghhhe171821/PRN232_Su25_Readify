using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Comments;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ReadifyDbContext _context;
        public CommentsController(ReadifyDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetCommentsByBookId/{bookId}")]
        public async Task<IActionResult> GetCommentsByBookId(int bookId, int pageNumber = 1, int pageSize = 5)
        {
            var book = await _context.Books.SingleOrDefaultAsync(b => b.Id == bookId);
            if(book == null) return NotFound();

            var totalComments = await _context.Comment
                .Where(c => c.BookId == bookId)
                .CountAsync();

            var comments = await _context.Comment
                .Include(c => c.User)
                .Where(c => c.BookId == bookId)
                .OrderByDescending(c => c.CreateDate) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var result = new
            {
                TotalComments = totalComments,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Comments = comments
            };

            return Ok(result);
        }
        [HttpPost("AddNewComments")]
        public async Task<IActionResult> AddNewComment([FromBody] AddCommentDto comment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var bookExists = await _context.Books.AnyAsync(b => b.Id == comment.BookId);

            if(!bookExists) return BadRequest();

            var userExists = await _context.Users.AnyAsync(u => u.Id.Equals(comment.UserId));

            if (!userExists) return BadRequest();

            try
            {
                var newComment = new Comment
                {
                    BookId = comment.BookId,
                    UserId = comment.UserId,
                    Content = comment.Content,
                    CreateDate = DateTime.Now,
                    IsActive = true
                };

                _context.Comment.Add(newComment);
                await _context.SaveChangesAsync();

                return Ok(newComment);
            }
            catch (Exception ex)
            {   
                return BadRequest(ex.Message);
            }
        }

    }
}
