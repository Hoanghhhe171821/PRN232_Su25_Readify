namespace PRN232_Su25_Readify_Web.Models.BookReport
{
    public class ManagerDecisionViewModel
    {
        public int ReportId { get; set; }
        public string BookTitle { get; set; }
        public string AuthorResponse { get; set; }  // lý do từ chối của tác giả
        public bool AcceptAuthorDenial { get; set; } // manager đồng ý với tác giả không
        public string Note { get; set; } // ghi chú của manager
    }
}
