using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Comment : BaseId
    {
        public string? Content { get; set; }

        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

    }
}
