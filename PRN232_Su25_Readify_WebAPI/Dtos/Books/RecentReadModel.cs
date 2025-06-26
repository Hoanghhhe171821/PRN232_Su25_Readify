using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Books
{
    public class RecentReadModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public int BookId { get; set; }
    }
}
