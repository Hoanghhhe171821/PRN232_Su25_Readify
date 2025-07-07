using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Net;
using System.Web;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ReadifyDbContext _context;
        private readonly IMail _mail;
        private const int PointVND = 1000;

        public AuthService(SignInManager<AppUser> signInManager, IMail mail,
            UserManager<AppUser> userManager, IJwtService jwtService,
            IConfiguration configuration, ReadifyDbContext context )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtService = jwtService;
            _configuration = configuration;
            _context = context;
            _mail = mail;
        }

        public async Task<AuthResult> LoginAsync(LoginDtoRequest login, string userAgent)
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

            var authResult = await _jwtService.GenerateTokenJWT(user,userAgent);

            await _userManager.ResetAccessFailedCountAsync(user);
            return authResult;
        }

        public async Task<string> RegisterAsync(RegisterDtoRequest register)
        {
            var isMail = await _userManager.FindByEmailAsync(register.Email);
            var isUserName = await _userManager.FindByNameAsync(register.UserName);
            if (isMail != null) { throw new BRException("Email is already exist"); }
            if (isUserName != null) { throw new BRException("UserName is already exist"); }


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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            var backendUrl = _configuration["Backend:AppUrl"];
            var confirmationLink = $"{backendUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";
            Console.WriteLine(confirmationLink);
            Console.WriteLine($"Token {token}");
            await _userManager.AddToRoleAsync(user, "User");
            await _mail.SendMailConfirmAsync(register.Email, confirmationLink);

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

        public async Task RemoveRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new BRException("Refresh token không hợp lệ.");

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken 
            && !rt.IsRevoked);

            if(token == null) { throw new BRException($"Unable to remove refresh token"); }

            token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<AccountDto>> ListAccountsAsync(string? keyword, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.UserName.Contains(keyword) || u.Email.Contains(keyword));
            }

            var totalItems = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<AccountDto>();

            foreach (var user in users)
            {
                // NOTE: Không truy cập song song vào DbContext
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new AccountDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Points = user.Points,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd?.DateTime,
                    Roles = roles.ToList()
                });
            }

            return new PagedResult<AccountDto>
            {
                Items = result,
                CurrentPage = page,
                TotalItems = totalItems,
                PageSize = pageSize
            };
        }

        public async Task<string> TopUpCoints(int points,string userId)
        {
            var validPoints = new[] { 50, 100, 200, 500 };
            if (!validPoints.Contains(points))
            {
                throw new BRException("Chỉ được phép nạp: 50, 100, 200 hoặc 500 xu.");
            }

            int vndAmount = points * PointVND;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new BRException("Không xác định được người dùng");
            }
            user.Points += vndAmount;

            var result = await _context.SaveChangesAsync();
            if (result <= 0)
                throw new BRException("Nạp xu thất bại. Vui lòng thử lại.");

            return $"Nạp thành công {points} xu (tương đương {vndAmount:N0} VNĐ).";
        }
    }
}
