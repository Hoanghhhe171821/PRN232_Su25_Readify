namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Cart : BaseId
    {
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
