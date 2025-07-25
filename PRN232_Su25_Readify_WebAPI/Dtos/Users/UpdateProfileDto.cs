namespace PRN232_Su25_Readify_WebAPI.Dtos.Users
{
    public class UpdateProfileDto
    {
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
