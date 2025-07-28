namespace PRN232_Su25_Readify_WebAPI.Dtos.Authors
{
    public class RoyaltyTransactionDto
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public bool IsPaid { get; set; }
        public int? OrderId { get; set; }
    }
}
