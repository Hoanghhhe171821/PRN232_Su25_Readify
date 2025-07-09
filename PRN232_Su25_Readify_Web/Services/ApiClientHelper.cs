namespace PRN232_Su25_Readify_Web.Services
{
    public class ApiClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public ApiClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _contextAccessor = httpContextAccessor;
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
            client.BaseAddress = new Uri("https://localhost:7267");
            return client;
        }
    }
}
