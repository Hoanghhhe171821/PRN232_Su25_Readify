namespace PRN232_Su25_Readify_WebAPI.Dtos.Order
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int UnitPrice { get; set; }

        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Author { get; set; }

    }

}
