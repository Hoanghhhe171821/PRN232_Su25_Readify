namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class OrderItem : BaseId
    {
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }
    
    }
}
