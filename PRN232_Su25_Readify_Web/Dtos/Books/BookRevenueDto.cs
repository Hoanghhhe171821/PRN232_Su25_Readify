namespace PRN232_Su25_Readify_Web.Dtos.Books
{
    public class BookRevenueDto
    {
        public int No { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public int TotalRevenue { get; set; }
    }
}
