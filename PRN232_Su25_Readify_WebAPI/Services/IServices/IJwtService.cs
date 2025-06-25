using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IJwtService
    {
        public Task<AuthResult> GenerateTokenJWT(AppUser user);
        public Task<(string token, DateTime expriseAt)> GenerateAccessToken(AppUser user);
        public Task<ClaimsPrincipal> ValidateToken(string token);
        public Task<(string token, DateTime expriseAt)> RefreshAccessToken(string refreshToken);
        Task<bool> RemoveRefreshTokenAsync(string refreshToken);
    }
}
