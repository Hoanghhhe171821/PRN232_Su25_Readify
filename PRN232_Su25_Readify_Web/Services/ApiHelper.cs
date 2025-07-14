using System.Text.Json;

namespace PRN232_Su25_Readify_Web.Services
{
    public static class ApiHelper
    {
        public static async Task<(bool Success, string? Data, string? ErrorMessage)> PostAsync<T>(string url, T payload,
            IHttpClientFactory clientFactory, string? userAgent = null)
        {
            using var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7267"); // hoặc truyền baseUrl nếu cần

            try
            {
                if (!string.IsNullOrEmpty(userAgent))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
                }

                var response = await client.PostAsJsonAsync(url, payload);

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, content, null);
                }

                // Parse lỗi JSON nếu có
                var errors = ParseErrorMessages(content);
                return (false, null, errors);
            }
            catch (Exception ex)
            {
                return (false, null, "Lỗi khi gọi API: " + ex.Message);
            }
        }

        private static string ParseErrorMessages(string jsonContent)
        {
            try
            {
                var json = JsonDocument.Parse(jsonContent);

                if (json.RootElement.TryGetProperty("errors", out var errorsProp))
                {
                    var errorMessages = new List<string>();

                    foreach (var error in errorsProp.EnumerateObject())
                    {
                        foreach (var message in error.Value.EnumerateArray())
                        {
                            errorMessages.Add(message.GetString());
                        }
                    }

                    return string.Join(" ", errorMessages);
                }

                if (json.RootElement.TryGetProperty("message", out var messageProp))
                {
                    return messageProp.GetString() ?? "Đã xảy ra lỗi.";
                }

                return "Không tìm thấy thông báo lỗi cụ thể.";
            }
            catch
            {
                return "Phản hồi không hợp lệ hoặc không phải JSON.";
            }
        }
    }

}
