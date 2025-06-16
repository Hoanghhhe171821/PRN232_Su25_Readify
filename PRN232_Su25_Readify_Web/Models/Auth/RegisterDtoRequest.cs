using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_Web.Models.Auth
{
    public class RegisterDtoRequest
    {
        [Required(ErrorMessage = "User is required.")]
        [StringLength(32, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 32 characters.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
