namespace PRN232_Su25_Readify_Web.Models.Account
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Points { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsAuthor { get; set; }
        public string? Bio { get; set; }
        public string? PublicEmail { get; set; }
        public string? PublicPhone { get; set; }
    }
}
