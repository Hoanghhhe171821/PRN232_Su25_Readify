namespace PRN232_Su25_Readify_WebAPI.Dtos.FeedbackRequest
{
    public class FeedbackRequestResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
