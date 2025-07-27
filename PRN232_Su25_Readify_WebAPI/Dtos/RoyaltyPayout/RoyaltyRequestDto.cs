namespace PRN232_Su25_Readify_WebAPI.Dtos.RoyaltyPayout
{
    public class RoyaltyRequestDto
    {
        public int Id { get; set; }
        public decimal RequestAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
