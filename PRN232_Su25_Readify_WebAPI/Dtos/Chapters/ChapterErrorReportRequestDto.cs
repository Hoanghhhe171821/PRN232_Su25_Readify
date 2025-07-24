namespace PRN232_Su25_Readify_WebAPI.Dtos.Chapters
{
    public class ChapterErrorReportRequestDto
    {
        public int ChapterId { get; set; }
        public int ErrorTypeId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

}
