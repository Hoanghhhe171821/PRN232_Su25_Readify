using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PRN232_Su25_Readify_Web.Middlewares;
using PRN232_Su25_Readify_Web.Services;
using System.Text;

namespace PRN232_Su25_Readify_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<ApiClientHelper>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();
            var jwtIssuer = "ReadifyAPI";
            var jwtAudience = "ReadifyMVC";
            var jwtKey = "N7jCmLuwYFTu36QDjXItF2ztEidFgpLu+LK2C5ZD5tM="; // TODO: di chuyển vào secrets

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true; // giữ token trong context
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.FromMinutes(1) // nới lỏng chút tránh lệch giờ
                    };

                    // Lấy token từ cookie frontend gửi lên
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.HttpContext.Request.Cookies["access_Token"];
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            // 401 Unauthorized
                            context.HandleResponse();
                            context.Response.Redirect("/Error/AccessDenied");
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            // 403 Forbidden
                            context.Response.Redirect("/Error/AccessDenied");
                            return Task.CompletedTask;
                        }
                    };
                });


            builder.Services.AddAuthentication("Bearer");
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMiddleware<JwtMiddleware>();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
