using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class RecentRead 
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int? ChapterId { get; set; }
        public Chapter? Chapter { get; set; }

        public DateTime DateRead { get; set; } = DateTime.UtcNow;
    }
}
