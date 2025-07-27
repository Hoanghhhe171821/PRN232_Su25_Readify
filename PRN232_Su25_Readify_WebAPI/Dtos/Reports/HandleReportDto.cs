namespace PRN232_Su25_Readify_WebAPI.Dtos.Reports
{
    public class HandleReportDto
    {
        public int ReportId { get; set; }
        public string Action { get; set; } // "warning", "lock", "resolve"
        public string? Note { get; set; }
    }

}
