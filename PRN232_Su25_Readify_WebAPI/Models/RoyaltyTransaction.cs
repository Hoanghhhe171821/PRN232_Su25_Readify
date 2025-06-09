namespace PRN232_Su25_Readify_WebAPI.Models
{
    // doanh thu cua tung cuon sach
    public class RoyaltyTransaction : BaseId
    {
        public int Amount { get; set; } // doanh thu thu da trich % tu sach
        public bool IsPaid { get; set; }

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }
    }
}
