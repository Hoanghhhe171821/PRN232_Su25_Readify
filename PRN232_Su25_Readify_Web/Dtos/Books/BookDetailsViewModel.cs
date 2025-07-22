using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos.Books
{
    public class BookDetailsViewModel
    {
        public string UserId { get; set; }
        public Book Book{ get; set; }
        public int ChapterQuantity { get; set; }
        public bool isFavorite { get; set; }
        public List<Book> RelatedBooks { get; set; }
        public List<ChapterDto> ChapterDto { get; set; }
        public RecentedReadChapters LastRead { get; set; }
        public PagedResult<Comment> PagedComments { get; set; }
    }
    public class ChapterDto
    {
        public Chapter Chapter { get; set; }
        public bool isRead { get; set; }
    }
    public class RecentedReadChapters
    {
        public int ChapterId { get; set; }
        public DateTime DateRead { get; set; }
        public int ChapterOrder { get; set; }
    }
}
