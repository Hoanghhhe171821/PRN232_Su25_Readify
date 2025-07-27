namespace PRN232_Su25_Readify_WebAPI.Dtos.TopUpCoints
{
    public class PaymentHistoryResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Points { get; set; }
        public string MoMoRequestId { get; set; }
        public string MoMoOrderId { get; set;  }
        public decimal Amount { get; set; }
        public String Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
