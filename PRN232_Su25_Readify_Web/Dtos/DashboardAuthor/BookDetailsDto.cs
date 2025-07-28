using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos.DashboardAuthor
{
    public class BookDetailsDto
    {
        public Book Book { get; set; }
        public List<Chapter> Chapters {get;set;}
        public ChapterUploadRequest Request { get; set; }
        public IFormFile File { get; set; }
        public int BookId { get; set; }
        public int ChapterOrder { get; set; }
        public string ChapterTitle { get; set; }
    }
    public class ChapterUploadRequest
    {
        public IFormFile File { get; set; }
        public int BookId { get; set; }
        public int ChapterOrder { get; set; }
        public string ChapterTitle { get; set; }
    }
}
