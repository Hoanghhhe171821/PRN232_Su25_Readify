using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_Web.Dtos.Books;

namespace PRN232_Su25_Readify_Web.Components
{
    public class FavoriteCountViewComponent : ViewComponent
    {
        private readonly HttpClient _httpClient;
        public FavoriteCountViewComponent(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7267/");
        }
        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {            
            //Seeding user
             userId = "0aece579-7768-4515-9f25-08e10f0e7032";

            if (string.IsNullOrEmpty(userId)) return View(0);
            var favoriteResult = await GetApiDataAsync<JObject>($"api/Books/GetUserFavorites?userId={userId}");
            var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();
            var favoritesCount = favoriteBooks.Count();

            return View(favoritesCount);
        }
        //Get data by Api Url
        private async Task<T> GetApiDataAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Báo l?i n?u API tr? l?i

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }
    }
}
