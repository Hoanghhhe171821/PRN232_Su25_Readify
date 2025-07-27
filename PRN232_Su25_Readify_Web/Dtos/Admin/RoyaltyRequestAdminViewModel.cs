namespace PRN232_Su25_Readify_Web.Dtos.Admin
{
    public class RoyaltyRequestAdminViewModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public decimal RequestAmount { get; set; }
        public DateTime CreateDate { get; set; }
        public int Status { get; set; }
    }
}
