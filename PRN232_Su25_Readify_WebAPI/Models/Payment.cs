using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Payment : BaseId
    {
        public int? Amount { get; set; }
        public string? PaymentMethod { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    }
}
