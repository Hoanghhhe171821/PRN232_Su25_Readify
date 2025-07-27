namespace PRN232_Su25_Readify_Web.Dtos.Books
{
    public class ChapterErrorReportDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ChapterTitle { get; set; }
        public string ErrorTypeName { get; set; }
        public string UserEmail { get; set; }
    }
}
