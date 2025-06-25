using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Net;
using System.Text;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class CommentsController : Controller
    {
        private readonly HttpClient _httpClient;
        private string GetGivenAPIBaseUrl()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            string baseUrl = config["GivenAPIBaseUrl"];
            return baseUrl;
        }
        public CommentsController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri(GetGivenAPIBaseUrl());
        }
        [HttpPost]
        public async Task<IActionResult> Add(string userId, int bookId, string content)
        {
            //Seeding user
            userId = "0aece579-7768-4515-9f25-08e10f0e7032";
            //thay = login
            if (userId == null) return RedirectToAction("Index", "Home");

            if (string.IsNullOrEmpty(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống!";
                return RedirectToAction("BookDetails", "Books", new { bookId = bookId });
            }

            var comment = new Comment
            {
                UserId = userId,
                BookId = bookId,
                Content = content
            };
            try
            {
                var result = await PostApiDataAsync<Comment>("api/Comments/AddNewComments",comment);
            }
            catch (Exception ex) { }
            return RedirectToAction("BookDetails", "Books", new { bookId = bookId });
        }
        private async Task<T> GetApiDataAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            response.EnsureSuccessStatusCode(); // Báo lỗi nếu API trả lỗi

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }
        private async Task<T> PostApiDataAsync<T>(string url, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, contentData);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            var resultData = JsonConvert.DeserializeObject<T>(resultJson);

            return resultData;
        }
    }
}
