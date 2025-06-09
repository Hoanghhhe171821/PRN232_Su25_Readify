namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Chapter : BaseId
    {
        public string? Title { get; set; }

        public string? FilePath { get; set; }

        public int? ChapterOrder { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }
        
        public ICollection<ChapterError>? Errors { get; set; }
    }
}
