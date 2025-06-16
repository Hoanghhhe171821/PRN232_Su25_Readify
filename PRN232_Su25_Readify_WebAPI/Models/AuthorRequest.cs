using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class AuthorRequest : BaseId
    {
        public string Bio { get; set; }
        public string FullName { get; set; }
        public StatusRequest Status { get; set; }
        
        public DateTime? ReviewAt { get; set; }
        public string? AdminResponse { get; set; }
        public string? ReviewBy { get; set; }
        [ForeignKey("ReviewBy")]
        public AppUser? Admin { get;set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    }
}
