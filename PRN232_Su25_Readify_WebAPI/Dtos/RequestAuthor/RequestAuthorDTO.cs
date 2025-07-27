namespace PRN232_Su25_Readify_WebAPI.Dtos.RequestAuthor
{
    public class RequestAuthorDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string CCCD { get; set; }
        public DateTime BirthDate { get; set; }
        public string Username { get; set; }
        public string BankAccount { get; set; }
        public string Reason { get; set; }
        public int IsApproved { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
