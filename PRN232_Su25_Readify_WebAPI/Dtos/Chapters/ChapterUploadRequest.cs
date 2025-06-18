namespace PRN232_Su25_Readify_WebAPI.Dtos.Chapters
{
    public class ChapterUploadRequest
    {
        public IFormFile File { get; set; }
        public int BookId { get; set; }
        public int ChapterOrder { get; set; }
        public string ChapterTitle { get; set; }
    }
}
