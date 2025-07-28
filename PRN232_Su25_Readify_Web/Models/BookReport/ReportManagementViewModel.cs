namespace PRN232_Su25_Readify_Web.Models.BookReport
{
    public class ReportManagementViewModel
    {
        public int ReportId { get; set; }
        public string BookTitle { get; set; }
        public string ReporterName { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ManagerNote { get; set; }
        public string AuthorDenialReason { get; set; }
        public int BookId { get; set; }
    }
}
