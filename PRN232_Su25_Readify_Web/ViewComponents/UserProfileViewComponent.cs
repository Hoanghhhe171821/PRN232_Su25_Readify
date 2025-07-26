using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_Web.Models.Account;

namespace PRN232_Su25_Readify_Web.ViewComponents
{
    public class UserProfileViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string apiUrl = "https://localhost:7267/api/users/profile";

        public UserProfileViewComponent(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_Token"];
                if (string.IsNullOrEmpty(token))
                    return View<ProfileViewModel>("Default", null);

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return View<ProfileViewModel>("Default", null);

                var json = await response.Content.ReadAsStringAsync();

                var profile = System.Text.Json.JsonSerializer.Deserialize<ProfileViewModel>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View<ProfileViewModel>("Default", profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserProfileVC ERROR] {ex.Message}");
                return View<ProfileViewModel>("Default", null);
            }
        }


    }
}
