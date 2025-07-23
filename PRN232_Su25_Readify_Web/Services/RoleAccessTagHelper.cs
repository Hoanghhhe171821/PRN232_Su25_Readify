using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace PRN232_Su25_Readify_Web.Services
{
    [HtmlTargetElement("role-access")]
    public class RoleAccessTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public string Roles { get; set; } = string.Empty;

        public RoleAccessTagHelper(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _contextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                output.SuppressOutput();
                return;
            }

            if (string.IsNullOrWhiteSpace(Roles))
                return;

            var requiredRoles = Roles.Split(',')
                                     .Select(r => r.Trim())
                                     .Where(r => !string.IsNullOrWhiteSpace(r))
                                     .ToList();

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type.EndsWith("/role"))
                .Select(c => c.Value)
                .ToList();

            Console.WriteLine("RequiredRoles: " + string.Join(", ", requiredRoles));
            Console.WriteLine("UserRoles: " + string.Join(", ", userRoles));

            if (!userRoles.Any(ur => requiredRoles.Contains(ur, StringComparer.OrdinalIgnoreCase)))
            {
                output.SuppressOutput();
            }
        }
    }
}
