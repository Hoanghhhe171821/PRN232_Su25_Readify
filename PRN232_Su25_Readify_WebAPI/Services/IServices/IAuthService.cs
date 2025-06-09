using PRN232_Su25_Readify_WebAPI.Dtos.Auths;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDtoRequest register);
        Task<AuthResult> LoginAsync(LoginDtoRequest login);
        Task<string> ForgotPassword(ForgotDtoRequest forgotPassword);
        Task<string> ResetPassword(ResetPasswordDto model);
    }
}
