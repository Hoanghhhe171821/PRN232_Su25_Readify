using System.Text.Json.Serialization;
using PRN232_Su25_Readify_WebAPI.Models.Enum;

namespace PRN232_Su25_Readify_Web.Models.BookReport
{
    public class AuthorPendingWarningViewModel
    {
        [JsonPropertyName("id")]
        public int ReportId { get; set; }
        public string BookTitle { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public BookReportStatus Status { get; set; }
    }
}
