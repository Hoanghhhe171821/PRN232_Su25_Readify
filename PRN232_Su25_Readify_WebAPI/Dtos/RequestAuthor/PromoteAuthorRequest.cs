namespace PRN232_Su25_Readify_WebAPI.Dtos.RequestAuthor
{
    public class PromoteAuthorRequest
    {
        public string FullName { get; set; }
        public string CCCD { get; set; }
        public DateTime BirthDate { get; set; }
        public string BankAccount { get; set; }
        public string Reason { get; set; }
    }
}
