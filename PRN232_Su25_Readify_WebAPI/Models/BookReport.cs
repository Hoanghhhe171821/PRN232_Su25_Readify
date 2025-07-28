using PRN232_Su25_Readify_WebAPI.Models.Enum;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class BookReport
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string ReporterId { get; set; }
        public string Reason { get; set; } // e.g. "Bản quyền", "Nội dung không phù hợp"
        public string Description { get; set; }
        public BookReportStatus Status { get; set; } = BookReportStatus.PENDING;
        public string? ManagerNote { get; set; }
        public string? AuthorResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Book Book { get; set; }
        public AppUser Reporter { get; set; }
    }

}
