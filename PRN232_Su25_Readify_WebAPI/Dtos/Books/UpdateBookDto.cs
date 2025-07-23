using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Books
{
    public class UpdateBookDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        public bool IsFree { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Price must be non-negative.")]
        public int Price { get; set; }

        public int UnitInOrder { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0.0, 1.0, ErrorMessage = "RoyaltyRate must be between 0 and 1.")]
        public decimal RoyaltyRate { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public DateTime? UpdateDate { get; set; } = DateTime.Now;

    }
}
