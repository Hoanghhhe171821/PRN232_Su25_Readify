namespace PRN232_Su25_Readify_WebAPI.Dtos.Chapters
{
    public class ChapterErrorReportDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
        public string ChapterTitle { get; set; } = string.Empty;
        public string ErrorTypeName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

}
