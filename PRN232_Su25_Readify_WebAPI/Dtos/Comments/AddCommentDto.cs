using PRN232_Su25_Readify_WebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Comments
{
    public class AddCommentDto
    {
        [Required]
        public int BookId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
