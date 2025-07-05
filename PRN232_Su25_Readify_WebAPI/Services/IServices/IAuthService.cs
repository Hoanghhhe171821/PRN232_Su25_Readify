using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDtoRequest register);
        Task<AuthResult> LoginAsync(LoginDtoRequest login);
        Task<string> ForgotPassword(ForgotDtoRequest forgotPassword);
        Task<string> ResetPassword(ResetPasswordDto model);
        Task RemoveRefreshTokenAsync(string refreshToken);
        Task<PagedResult<AccountDto>> ListAccountsAsync(string? keyword, int page = 1, int pageSize = 10);
        Task<string> TopUpCoints(int points,string userId);
    }
}
