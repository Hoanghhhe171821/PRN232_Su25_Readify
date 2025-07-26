using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Author : BaseId
    {
        [StringLength(50)]
        public string Name { get; set; }
        public string? Bio { get; set; }
        public string? Website { get; set; }
        public string? PublicEmail { get; set; }   // Thêm email công khai
        public string? PublicPhone { get; set; }   // Thêm sđt công khai

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
        public ICollection<Book>? Books { get; set; }
    }
}
