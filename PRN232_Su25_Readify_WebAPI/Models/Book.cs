using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Book : BaseId
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsFree { get; set; }
        public int Price { get; set; }
        public int UnitInOrder { get; set; } = 0;
        public string? ImageUrl { get; set; }
        [Range(0,0.9)]
        public decimal? RoyaltyRate { get; set; } // % trich cho tac gia

        public int AuthorId { get; set; }
        public Author? Author { get; set; }
        public bool IsPublished { get; set; } = false; // Sach da duoc xuat ban
        public string? UploadedBy { get; set; }
        [ForeignKey("UploadedBy")]
        public AppUser? AppUser { get; set; }

        public ICollection<BookCategory>? BookCategories { get; set; }
        public ICollection<Chapter>? Chapters { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<RoyaltyTransaction>? RoyaltyTransactions { get; set; }
        public ICollection<BookLicense>? BookLicenses { get; set; }
    }
}
