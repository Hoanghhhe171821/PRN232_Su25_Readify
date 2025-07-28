namespace PRN232_Su25_Readify_WebAPI.Dtos.Reports
{
    public class AuthorResponseDto
    {
        public int ReportId { get; set; }
        public bool AcceptLock { get; set; }
        public string? Reason { get; set; }
    }

}
