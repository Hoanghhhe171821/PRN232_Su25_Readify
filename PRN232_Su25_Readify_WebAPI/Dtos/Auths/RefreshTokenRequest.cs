using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Auths
{
    public class RefreshTokenRequest
    {
        public string? AccessToken { get; set; }
        [Required]
        public string SessionsId { get; set; }
    }
}
