namespace PRN232_Su25_Readify_Web.Models.BookReport
{
    public class ManageReportViewModel
    {
        public int ReportId { get; set; }
        public string BookTitle { get; set; }
        public string ReporterName { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string AuthorResponse { get; set; }
        public string Action { get; set; } // warning, lock, resolve
        public string Note { get; set; }
        public bool AcceptAuthorDenial { get; set; }
    }
}
