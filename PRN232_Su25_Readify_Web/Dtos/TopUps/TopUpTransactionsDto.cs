namespace PRN232_Su25_Readify_Web.Dtos.TopUps
{
    public class TopUpTransactionsDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public int Points { get; set; }
        public int Amount { get; set; }
        public string? MoMoOrderId { get; set; } = null!;
        public string? MomoRequestId { get; set; } = null!;
        public string? QrCodeUrl { get; set; } = null!;
        public string? PaymentUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreateDate { get; set; }
    }
}
