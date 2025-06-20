using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_Web.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Net;
using static System.Reflection.Metadata.BlobBuilder;

namespace PRN232_Su25_Readify_Web.Controllers
{
    [Route("[Controller]")]
    public class BooksController : Controller
    {
        private readonly HttpClient _httpClient;
        public BooksController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7267/");
        }
        [HttpGet("BookList")]
        public async Task<IActionResult> BookList(int page = 1, string searchTitle = null,
            List<int> cateIds = null, string orderBy = "Desc",bool isFree = false)
        {
            //Seeding User
            var userId = "0aece579-7768-4515-9f25-08e10f0e7032";

            var url = $"api/Books/GetAllBooks?page={page}&searchTitle={searchTitle}&orderBy={orderBy}&isFree={isFree}";
            if (cateIds != null && cateIds.Any())
            {
                url += "&" + string.Join("&", cateIds.Select(id => $"cateIds={id}"));
            }

            //Get Book by API
            var booksJsonResult = await GetApiDataAsync<JObject>(url);
            
            var books = booksJsonResult["items"].ToObject<List<BookViewModel>>();
            var totalItems = booksJsonResult["totalItems"].ToObject<int>();
            var pageSize = booksJsonResult["pageSize"].ToObject<int>();
            var totalPage = (int)Math.Ceiling((double)totalItems / pageSize);
            //Get Cate by API
            var categories = await GetApiDataAsync<List<Category>>("api/Categories/GetAllCategories");
            //Get Favor by API
            var favorites = await GetApiDataAsync<List<int>>($"api/Books/GetUserFavorites?userId={userId}");
            //Gán isFavor
            foreach (var book in books)
            {
                book.IsFavorite = favorites.Contains(book.Id);
            }

            var model = new BookListViewModel
            {
                PagedBooks = new PagedResult<BookViewModel>
                {
                    Items = books,
                    TotalItems = totalItems,
                    PageSize = pageSize,
                    PageNumber = page,
                    TotalPage = totalPage
                },
                Categories = categories.ToList(),
                OrderBy = orderBy,
                SearchTitle = searchTitle,
                IsFree = isFree,
                UserId = userId
            };
            return View(model);
        }
        [HttpGet("BookDetails/{bookId}")]
        public async Task<IActionResult> BookDetails(int bookId)
        {
            var book =await GetApiDataAsync<Book>($"api/Books/GetBookById/{bookId}");
            if (book == null) return RedirectToAction("BookList", "Books");

            var chapterQuan = book.Chapters.Count();
            var result = new BookDetailsViewModel
            {
                Book = book,
                ChapterQuantity = chapterQuan
            };
            return View(result);
        }
        [HttpGet("Read")]
        public async Task<IActionResult> Read( int bookId, int chapterOrder)
        {
            if (bookId == null && chapterOrder == null) return RedirectToAction("BookDetails", "Books");

            var book = await GetApiDataAsync<Book>($"api/Books/GetBookById/{bookId}");
            if (book == null ) return RedirectToAction("BookList", "Books");
            //Kiểm tra sách free hoặc đã mua
            if (book.IsFree == false) return RedirectToAction("BookList", "Books");


            var chapters = await GetApiDataAsync<List<Chapter>>($"api/Books/GetAllChapterByBookId/{bookId}");
            if(chapters == null) return RedirectToAction("BookDetails", "Books", new { bookId = bookId });

            var query = await GetApiDataAsync<ReadViewModel>($"GetChapter?bookId={bookId}&chapterOrder={chapterOrder}");
            if (query == null) return RedirectToAction("BookDetails", "Books", new { bookId = bookId });
            var result = new ReadViewModel
            {
                Book = book,
                ChapterOrder = chapterOrder,
                Content = query.Content,
                Title = query.Title,
                Chapters = chapters
            };
            return View(result);
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

    }
}
