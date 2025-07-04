﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Net;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService,
            UserManager<AppUser> userManager,
            IJwtService jwtService)
        {
            _authService = authService;
            _userManager = userManager;
            _jwtService = jwtService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDtoRequest login)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                throw new ValidationEx(errors);
            }

            var result = await _authService.LoginAsync(login);

            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId,
           [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                throw new BRException("Invalid email confirmation request.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { throw new NFoundEx("User not found"); }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("Email confirmed successfully!");
            }

            throw new BRException("Email confirmation failed.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDtoRequest register)
        {
            var result = await _authService.RegisterAsync(register);
            return Ok(result); // result có thể là chuỗi thông báo hoặc object
        }

        [HttpGet("verify-reset-password")]
        public async Task<IActionResult> VerifyResetPasswordToken([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                throw new BRException("Missing email or token");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BRException("Invalid user");

            var decodedToken = WebUtility.UrlDecode(token);

            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                decodedToken
            );

            if (!isValid)
                throw new BRException("Invalid or expired token.");

            return Ok(new { message = "redirectUrl " });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotDtoRequest dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                throw new ValidationEx(errors);
            }
            var result = await _authService.ForgotPassword(dto);
            return Ok(result);

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                throw new ValidationEx(errors);
            }
            var result = await _authService.ResetPassword(dto);
            return Ok(result);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshTokenRequest dto)
        {

            if (dto.AccessToken != null)
            {
                var principal = _jwtService.ValidateToken(dto.AccessToken);
                if (principal == null) throw new UnauthorEx("Invalid accessToken, Please login again");
            }

            var newAccessToken = await _jwtService.RefreshAccessToken(dto.RefreshToken);

            return Ok(new AuthResult
            {
                Token = newAccessToken.token,
                RefreshToken = dto.RefreshToken,
                ExpriseAt = newAccessToken.expriseAt
            });
        }
        [HttpPost("logout")]        
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new BRException("Refresh token là bắt buộc");

            await _authService.RemoveRefreshTokenAsync(refreshToken);

            return Ok();
        }
        [Authorize]
        [HttpPost("TopUp")]
        public async Task<IActionResult> TopUpCoins([FromBody] TopUpRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new UnauthorEx("Không xác định được người dùng từ token");

            var message = await _authService.TopUpCoints(request.Points, userId);
            return Ok(new { Message = message });
        }
        
    }
}
