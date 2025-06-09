namespace PRN232_Su25_Readify_WebAPI.Models
{
    // lich su rut tien
    public class RoyaltyPayoutTransaction : BaseId
    {
        public int RoyaltyTransactionId { get; set; }
        public RoyaltyTransaction? RoyaltyTransaction { get; set; }
    
        public int RoyaltyPayoutRequestId { get; set; }
        public RoyaltyPayoutRequest? RoyaltyPayoutRequest { get; set; }
    }
}
