using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Authors
{
    public class AuthorProfileDto
    {
        public string Name { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public List<BookSummary> Books { get; set; } = new();
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

}
