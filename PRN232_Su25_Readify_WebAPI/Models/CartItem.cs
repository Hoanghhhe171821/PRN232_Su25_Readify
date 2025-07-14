namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class CartItem : BaseId
    {
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }
    }
}
