namespace PRN232_Su25_Readify_Web.Models.Author
{
    public class AuthorProfileViewModel
    {
        public string Name { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Website { get; set; }
        public List<BookSummary> Books { get; set; } = new();
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class BookSummary
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
    }
}
