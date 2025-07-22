namespace PRN232_Su25_Readify_Web.Dtos.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int TotalAmount { get; set; }
        public string Status { get; set; } // Assuming Status is a string representation of the enum
        public string UserEmail { get; set; } // Assuming you want to include user email in the DTO
        public DateTime CreateDate { get; set; } // Assuming you want to include creation date
    }
}
