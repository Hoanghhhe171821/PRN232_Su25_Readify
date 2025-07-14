using Microsoft.AspNetCore.Identity;
using Octokit;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        public JwtMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, UserManager<AppUser> userManager, IJwtService tokenService)
        {
            var token = context.Request.Cookies["access_Token"];
            if (!string.IsNullOrEmpty(token))
            {
                var principal = await tokenService.ValidateToken(token);
                if (principal != null)
                {
                    context.User =  principal;

                    // Nếu cần: kiểm tra user tồn tại thật không
                    var userId =  principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        context.User = new ClaimsPrincipal(); // clear nếu user bị xoá
                    }
                }
            }

            await _next(context);
        }
    }
}
