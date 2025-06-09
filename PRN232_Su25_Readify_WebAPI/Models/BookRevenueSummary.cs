namespace PRN232_Su25_Readify_WebAPI.Models
{
    // doanh thu cua 1 cuon sach
    public class BookRevenueSummary : BaseId
    {
        public int TotalSold { get; set; }
        public int TotalRevenue { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }
    }
}
