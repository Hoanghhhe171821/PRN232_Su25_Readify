using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Order : BaseId
    {
        public int TotalAmount { get; set; }
        public StatusOrder Status { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
        
    }
}
