using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Middlewares;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                 {
                     options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                     options.JsonSerializerOptions.WriteIndented = true;
                 });
builder.Services.Configure<MoMoConfig>(
    builder.Configuration.GetSection("MoMo"));

builder.Services.AddDbContext<ReadifyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
var emailConfig = configuration.GetSection("EmailConfiguration")
                                           .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddHttpClient();
// DI Service
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITopUpTransService, TopUpTransService>();
builder.Services.AddScoped<IRoyaltyTransactionService, RoyaltyTransactionService>();
builder.Services.AddScoped<IAuthorRevenueService, AuthorRevenueService>();
builder.Services.AddScoped<IBookRevenueService, BookRevenueService>();

builder.Services.AddScoped<IMail, MailService>();
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ReadifyDbContext>()
    .AddDefaultTokenProviders();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Readify API",
        Version = "v1"
    });

    // Cấu hình Swagger hỗ trợ JWT Bearer
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                        Nhập token theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcApp",
        builder => builder.WithOrigins("https://localhost:7154")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ReadifyAPI",
            ValidateAudience = true,
            ValidAudience = "ReadifyMVC",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("N7jCmLuwYFTu36QDjXItF2ztEidFgpLu+LK2C5ZD5tM=")),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role

        };
    });

//Cloudinary Configuration
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    return new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Use(async (context, next) =>
{
    Console.WriteLine($"➡️ {context.Request.Method} {context.Request.Path} - Authenticated: {context.User.Identity?.IsAuthenticated}");
    foreach (var claim in context.User.Claims)
    {
        Console.WriteLine($"➡️ Claim: {claim.Type} = {claim.Value}");
    }
    await next();
});
app.UseRouting();
app.UseCors("AllowMvcApp");
app.UseMiddleware<ExceptionMid>();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.MapControllers();
app.Run();
