using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Octokit;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Chapters;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChaptersController : ControllerBase
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repo;
        private readonly ReadifyDbContext _context;
        public ChaptersController(IConfiguration configuration, ReadifyDbContext context)
        {
            var token = configuration["GitHub:Token"];
            _owner = configuration["GitHub:Owner"];
            _repo = configuration["GitHub:Repo"];   

            _client = new GitHubClient(new ProductHeaderValue("PRN232_Su25_Readify_WebAPI"));
            _client.Credentials = new Credentials(token);
            _context = context;
        }
        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Ví dụ: lấy thông tin repo
                var repo = await _client.Repository.Get(_owner, _repo);
                return Ok(new
                {
                    repo.FullName,
                    repo.Private,
                    repo.Description
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("AddOrUpdateChapter")]
        public async Task<IActionResult> CreateOrUpdateChapterByBookId([FromForm] ChapterUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File is required.");

            var extension = Path.GetExtension(request.File.FileName).ToLower();
            if (extension != ".pdf")
                return BadRequest("Only .pdf files are allowed.");

            var book = await _context.Books.SingleOrDefaultAsync(b => b.Id == request.BookId);
            if (book == null)
                return BadRequest("BookId not found.");

            var bookName = book.Title.Replace(" ", "_");
            var folderPath = bookName;
            var fileName = $"{bookName}_Chapter_{request.ChapterOrder}{extension}";
            var repoFilePath = $"{folderPath}/{fileName}";

            // Convert file to base64
            string base64Content;
            using (var ms = new MemoryStream())
            {
                await request.File.CopyToAsync(ms);
                base64Content = Convert.ToBase64String(ms.ToArray());
            }

            // Check if file exists on GitHub
            bool fileExists = false;
            string existingSha = null;
            try
            {
                var existingContent = await _client.Repository.Content.GetAllContentsByRef(_owner, _repo, repoFilePath, "main");
                fileExists = true;
                existingSha = existingContent.First().Sha;
            }
            catch (NotFoundException)
            {
                // File does not exist – proceed to create
            }

            if (fileExists)
            {
                // Update file
                var updateRequest = new UpdateFileRequest($"Update {fileName}", base64Content, existingSha, "main");
                await _client.Repository.Content.UpdateFile(_owner, _repo, repoFilePath, updateRequest);
            }
            else
            {
                // Create new file
                var createRequest = new CreateFileRequest($"Add {fileName}", base64Content, "main");
                await _client.Repository.Content.CreateFile(_owner, _repo, repoFilePath, createRequest);
            }

            // Update or insert chapter metadata in DB
            var existingChapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.BookId == request.BookId && c.ChapterOrder == request.ChapterOrder);

            if (existingChapter != null)
            {
                existingChapter.Title = request.ChapterTitle;
                existingChapter.FilePath = repoFilePath;
                existingChapter.UpdateDate = DateTime.Now;
                _context.Chapters.Update(existingChapter);
            }
            else
            {
                var newChapter = new Chapter
                {
                    Title = request.ChapterTitle,
                    FilePath = repoFilePath,
                    ChapterOrder = request.ChapterOrder,
                    BookId = request.BookId,
                    IsActive = true,
                    CreateDate = DateTime.Now
                };
                _context.Chapters.Add(newChapter);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = fileExists ? "Chapter updated" : "Chapter created", path = repoFilePath });
        }


        [HttpGet("GetChapter")]
        public async Task<IActionResult> GetChapter(int bookId, int chapterOrder)
        {
            var chapter = await _context.Chapters.Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.BookId == bookId && c.ChapterOrder == chapterOrder);

            if (chapter == null)
                return NotFound("Chapter not found.");

            try
            {
                var content = await _client.Repository.Content.GetAllContentsByRef(_owner, _repo, chapter.FilePath, "main");
                var base64 = content.First().Content;
                var fileBytes = Convert.FromBase64String(base64);

                return File(fileBytes, "application/pdf", Path.GetFileName(chapter.FilePath));
            }
            catch (NotFoundException)
            {
                return NotFound("File not found in GitHub.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("GetRecentedReadChapters")]
        public async Task<IActionResult> GetRecentedReadChapters(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            var isExistedBook =await _context.Books.AnyAsync(b => b.Id == bookId);
            if(!isExistedBook) return BadRequest("Book not exist!");

            var readChapterInfo = await _context.RecentRead
        .Where(rd => rd.UserId.Equals(userId) && rd.BookId == bookId)
        .Join(_context.Chapters,
              recentRead => recentRead.ChapterId,
              chapter => chapter.Id,
              (recentRead, chapter) => new 
              {
                  recentRead.ChapterId,
                  recentRead.DateRead,
                  chapter.ChapterOrder 
              })
        .ToListAsync();
            return Ok(readChapterInfo);
        }
    }
}
