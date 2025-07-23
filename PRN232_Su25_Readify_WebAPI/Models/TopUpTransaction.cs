using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class TopUpTransaction
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!; // Navigation property

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
