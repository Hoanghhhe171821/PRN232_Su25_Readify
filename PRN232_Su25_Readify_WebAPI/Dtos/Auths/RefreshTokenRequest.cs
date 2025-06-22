using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Auths
{
    public class RefreshTokenRequest
    {
        [Required] 
        public string? AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
