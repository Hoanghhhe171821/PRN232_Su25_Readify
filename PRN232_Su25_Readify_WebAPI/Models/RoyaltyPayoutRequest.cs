namespace PRN232_Su25_Readify_WebAPI.Models
{
    // yeu cau rut tien
    public class RoyaltyPayoutRequest : BaseId
    {
        public int RequestAmount { get; set; }
        public int ApprovedAmount { get; set; }
        public string? AdminFeedback { get; set; }
        public RoyaltyPayoutStatus Status { get; set; }

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public ICollection<RoyaltyPayoutTransaction>? RoyaltyPayoutTransactions { get; set; }
        
    }
}
