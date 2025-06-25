namespace PRN232_Su25_Readify_Web.Models.Account
{
    public class AccountDto
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? Email { get; set; } // nullable để tránh lỗi nếu API trả về null

        public int Points { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTime? LockoutEnd { get; set; } // nullable để tránh lỗi nếu JSON trả về null
        public List<string> Roles { get; set; } = new();
    }
}
