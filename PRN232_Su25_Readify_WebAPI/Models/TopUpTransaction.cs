namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class TopUpTransaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int Points { get; set; } // Số điểm quy đổi
        public int Amount { get; set; } // Số tiền tương ứng
        public string MoMoOrderId { get; set; } = null!;
        public string MoMoRequestId { get; set; } = null!;
        public string QrCodeUrl { get; set; } = null!;
        public string PaymentUrl { get; set; } = null!;
        public string Status { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
