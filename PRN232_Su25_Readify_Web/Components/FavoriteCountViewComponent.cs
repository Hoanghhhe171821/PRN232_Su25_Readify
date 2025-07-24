using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_Web.Dtos.Books;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System;

namespace PRN232_Su25_Readify_Web.Components
{
    public class FavoriteCountViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public FavoriteCountViewComponent(IHttpClientFactory factory)
        {
            _httpClientFactory = factory;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {            


            var favoriteResult = await GetAuthorizedApiDataAsync<JObject>("https://localhost:7267/api/Books/GetUserFavorites");
            if(favoriteResult == null) return View(0);

            var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();
            var favoritesCount = favoriteBooks.Count();

            return View(favoritesCount);
        }

        private async Task<T> GetAuthorizedApiDataAsync<T>(string fullUrl)
        {
            var token = Request.Cookies["access_Token"];
            if (string.IsNullOrEmpty(token)) return default;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(fullUrl);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }
    }
}
