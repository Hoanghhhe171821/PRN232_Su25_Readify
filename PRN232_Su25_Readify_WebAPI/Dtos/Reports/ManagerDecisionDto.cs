namespace PRN232_Su25_Readify_WebAPI.Dtos.Reports
{
    public class ManagerDecisionDto
    {
        public int ReportId { get; set; }
        public bool AcceptAuthorDenial { get; set; }
        public string? Note { get; set; }
    }
}
