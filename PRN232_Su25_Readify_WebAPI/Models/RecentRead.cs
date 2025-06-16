using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class RecentRead 
    {
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    
        public int BookId { get; set; }
        public Book? Book { get; set; }
    }
}
