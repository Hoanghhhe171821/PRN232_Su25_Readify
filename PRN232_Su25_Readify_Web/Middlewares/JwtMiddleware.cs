using PRN232_Su25_Readify_Web.Models.Auth;
using PRN232_Su25_Readify_Web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Middlewares
{
    public class JwtMiddleware 
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RequestDelegate _requestDelegate;
        private const string apiRefreshToken = "https://localhost:7267/api/Auth/refresh-token";

        public JwtMiddleware(IHttpClientFactory httpClientFactory, RequestDelegate requestDelegate)
        {
            _httpClientFactory = httpClientFactory;
            _requestDelegate = requestDelegate;
        }



        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Cookies["access_Token"];
            var sessionId = context.Request.Cookies["session_Id"];
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            bool shouldRefresh = false;

            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new JwtSecurityTokenHandler();

                try
                {
                    var token = handler.ReadJwtToken(accessToken);

                    if (token.ValidTo < DateTime.UtcNow)
                    {
                        shouldRefresh = true;
                    }
                }
                catch
                {
                    shouldRefresh = true;
                }
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                shouldRefresh = true;
            }

            if (shouldRefresh && !string.IsNullOrEmpty(sessionId))
            {

                var body = new
                {
                    AccessToken = accessToken, // có thể là null
                    SessionsId = sessionId,
                };

                var (success, data, errorMessage) = await ApiHelper.PostAsync
                    ("api/Auth/refresh-token", body, _httpClientFactory, userAgent);
                if (success && !string.IsNullOrEmpty(data))
                {
                    var result = JsonSerializer.Deserialize<AuthResult>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        context.Response.Cookies.Append("access_Token", result.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = result.ExpriseAt
                        });
                    }
                }
                else
                {
                    context.Response.Cookies.Delete("access_Token");
                    context.Response.Cookies.Delete("session_Id");
                    context.Response.Redirect("auths/login");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(context.Request.Cookies["access_Token"]))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(context.Request.Cookies["access_Token"]);
                    var identity = new ClaimsIdentity(token.Claims, "jwt");
                    context.User = new ClaimsPrincipal(identity);
                }
                catch
                {
                    // token lỗi => không làm gì
                }
            }

            await _requestDelegate(context);
        }

    }
}
