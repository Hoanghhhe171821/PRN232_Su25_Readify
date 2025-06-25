using PRN232_Su25_Readify_Web.Models.Auth;
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
            var refreshToken = context.Request.Cookies["refresh_Token"];

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
            else if (!string.IsNullOrEmpty(refreshToken))
            {
                shouldRefresh = true;
            }

            if (shouldRefresh && !string.IsNullOrEmpty(refreshToken))
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, apiRefreshToken);

                var body = new
                {
                    AccessToken = accessToken, // có thể là null
                    RefreshToken = refreshToken
                };

                request.Content = new StringContent(JsonSerializer.Serialize(body),
                    Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthResult>(json, new JsonSerializerOptions
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
                    context.Response.Cookies.Delete("refresh_Token");
                    context.Response.Redirect("/Login");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new JwtSecurityTokenHandler();

                try
                {
                    var token = handler.ReadJwtToken(accessToken);
                    var claimsIdentity = new ClaimsIdentity(token.Claims, "jwt");
                    context.User = new ClaimsPrincipal(claimsIdentity);
                }
                catch { }
            }

            await _requestDelegate(context);
        }

    }
}
