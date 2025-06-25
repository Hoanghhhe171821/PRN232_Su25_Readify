namespace PRN232_Su25_Readify_WebAPI.Dtos.Auths
{
    public class AccountDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public int Points { get; set; }
        public string Email { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
