using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Octokit;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Chapters;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
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
        [HttpPost("AddNewChapter")]
        public async Task<IActionResult> CreateChapterByBookId([FromForm] ChapterUploadRequest request)
        {
            //Validate file
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File is required.");
            //Validate extension
            var extension = Path.GetExtension(request.File.FileName).ToLower();

            if (extension != ".docx" && extension != ".txt")
                return BadRequest("Only .docx or .txt files are allowed.");
            //Find Book
            var book = await _context.Books.SingleOrDefaultAsync(b=> b.Id == request.BookId);
            if (book == null)
                return BadRequest("BookId not found.");
            //Folder form: [Book_Name]/[Book_Name]_Chapter_[int].[.txt / .docx]
            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
            var bookName = book.Title.Replace(" ", "_");
            var folderPath = bookName;
            var fileName = $"{bookName}_Chapter_{request.ChapterOrder}{fileExtension}";
            var repoFilePath = $"{folderPath}/{fileName}";

            string base64Content;
            using (var ms = new MemoryStream())
            {
                await request.File.CopyToAsync(ms);
                base64Content = Convert.ToBase64String(ms.ToArray());
            }
            //Kiểm tra folder tồn tại?
            try
            {
                await _client.Repository.Content.GetAllContentsByRef(_owner, _repo, folderPath, "main");
            }
            catch (NotFoundException)
            {
                // Folder chưa có - không cần tạo folder riêng, GitHub hiểu path khi tạo file
            }
            var createRequest = new CreateFileRequest($"Add {fileName}", base64Content, "main");
            var result = await _client.Repository.Content.CreateFile(_owner, _repo, repoFilePath, createRequest);

            //Lưu vào database
            var chapter = new Chapter
            {
                Title = request.ChapterTitle,
                FilePath = repoFilePath,
                ChapterOrder = request.ChapterOrder,
                BookId = request.BookId,
                IsActive = true,
                CreateDate = DateTime.Now
            };
            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Chapter created successfully", path = repoFilePath });
        }
        [HttpGet("GetChapter")]
        public async Task<IActionResult> GetChapter(int bookId, int chapterOrder)
        {
            Chapter chapter =await _context.Chapters.Include(c => c.Book).FirstOrDefaultAsync(c => c.BookId == bookId && c.Id == chapterOrder);
            if (chapter == null) return NotFound();

            try
            {
                var fileContent = await _client.Repository.Content.GetAllContentsByRef(
                                            _owner, _repo, chapter.FilePath, "main");
                var base64Content = fileContent.First().Content;
                var fileBytes = Convert.FromBase64String(base64Content);
                var text = System.Text.Encoding.UTF8.GetString(fileBytes);
                return Ok(new { chapter.Title, content = text });
            }
            catch (NotFoundException)
            {
                return NotFound("File not found in GitHub repository.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
