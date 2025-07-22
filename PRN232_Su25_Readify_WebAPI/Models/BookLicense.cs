using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class BookLicense
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public AppUser? User { get; set; }

        // Tham chiếu OrderItem
        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

    }

}
