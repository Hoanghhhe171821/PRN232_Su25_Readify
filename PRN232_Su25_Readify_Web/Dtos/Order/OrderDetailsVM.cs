using PRN232_Su25_Readify_Web.Dtos.Books;

namespace PRN232_Su25_Readify_Web.Dtos.Order
{
    public class OrderDetailsVM
    {
        public OrderDto Order { get; set; } = null!;
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

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
