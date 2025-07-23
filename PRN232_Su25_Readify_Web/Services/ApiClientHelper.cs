namespace PRN232_Su25_Readify_Web.Services
{
    public class ApiClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        public ApiClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _contextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public HttpClient CreateClientWithToken()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _contextAccessor.HttpContext?.Request.Cookies["access_Token"];

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers
                    .AuthenticationHeaderValue("Bearer", token);
            }
            var baseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7267";
            client.BaseAddress = new Uri("https://localhost:7267");
            return client;
        }
    }
}
