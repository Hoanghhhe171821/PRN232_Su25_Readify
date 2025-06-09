using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Net;
using System.Net.Http;
using System.Web;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager, IJwtService jwtService,
            IConfiguration configuration )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtService = jwtService;
            _configuration = configuration;
        }



        public async Task<AuthResult> LoginAsync(LoginDtoRequest login)
        {
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null) { throw new NFoundEx("User not found"); }
            // true ở đây cho phép khoá tài khoản lại sau nhiều lần đăng nhập sai
            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, true);

            if (user.LockoutEnabled && user.LockoutEnd.HasValue
                && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                throw new BRException($"Your account has locked untill" +
                    $" {user.LockoutEnd.Value}.");
            }

            if (!result.Succeeded)
                throw new BRException("Password or email is not correct");

            var isConfirm = await _userManager.IsEmailConfirmedAsync(user);

            if (!isConfirm)
            {
                throw new BRException("Your email has not confirm. Please check your email");
            }

            var authResult = await _jwtService.GenerateTokenJWT(user);

            await _userManager.ResetAccessFailedCountAsync(user);
            return authResult;
        }

        public async Task<string> RegisterAsync(RegisterDtoRequest register)
        {
            var isMail = await _userManager.FindByEmailAsync(register.Email);

            if (isMail != null) { throw new BRException("Email is already exist"); }

            var user = new AppUser
            {
                UserName = register.UserName,
                Email = register.Email,
            };

            var result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray()
                    );

                throw new ValidationEx(errors);
            }
            await _userManager.AddToRoleAsync(user, "User");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            var backendUrl = _configuration["Backend:AppUrl"];
            var confirmationLink = $"{backendUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";
            Console.WriteLine(confirmationLink);
            Console.WriteLine($"Token {token}");

            return "Register successfully! Please check your email to confirm.";
        }

        // tao request forgot Password
        public async Task<string> ForgotPassword(ForgotDtoRequest forgotPassword)
        {
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                throw new BRException("Email is not exist or not confirmed.");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebUtility.UrlEncode(token);

            var backendUrl = _configuration["Backend:AppUrl"];

            var resetLink = $"{backendUrl}/api/auth/verify-reset-password/{forgotPassword.Email}&token={encodedToken}";

            Console.WriteLine(resetLink);

            return "Password reset link has been sent to your email.";
        }

        public async Task<string> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) { throw new BRException("User not found"); }

            var decodedToken = WebUtility.UrlDecode(model.Token);
            var result = await _userManager.
                ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray()
                    );

                throw new ValidationEx(errors);
            }

            return "Password has been reset successfully.";
        }
    }
}
