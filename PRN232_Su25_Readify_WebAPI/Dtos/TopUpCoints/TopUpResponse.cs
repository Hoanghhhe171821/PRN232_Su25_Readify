namespace PRN232_Su25_Readify_WebAPI.Dtos.TopUpCoints
{
    public class TopUpResponse
    {
        public string QrCodeUrl { get; set; }
        public string PayUrl { get; set; }
        public int TransactionId { get; set; }
    }
}
