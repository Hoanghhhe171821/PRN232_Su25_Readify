using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Books
{
    public class CreateBookDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsFree { get; set; }

        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0.0, 1.0)]
        public decimal RoyaltyRate { get; set; }
        public bool IsPublished { get; set; } = false;

        [Required]
        public int AuthorId { get; set; }

        // List CategoryId người dùng chọn
        public List<int> CategoryIds { get; set; }
    }
}
