namespace PRN232_Su25_Readify_WebAPI.Dtos.Users
{
    public class UpdateProfileDto
    {
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? Avatar { get; set; }

        // Các trường dành cho tác giả
        public string? Bio { get; set; }
        public string? PublicEmail { get; set; }
        public string? PublicPhone { get; set; }
    }
}
