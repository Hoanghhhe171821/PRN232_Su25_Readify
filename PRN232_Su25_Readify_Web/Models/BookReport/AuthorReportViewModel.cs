namespace PRN232_Su25_Readify_Web.Models.BookReport
{
    public class AuthorReportViewModel
    {

        public int ReportId { get; set; }
        public string BookTitle { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string ManagerNote { get; set; }
        public bool AcceptLock { get; set; }
        public string DenialReason { get; set; }
    }

}
