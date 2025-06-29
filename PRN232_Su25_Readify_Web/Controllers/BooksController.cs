using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_Web.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Net;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;

namespace PRN232_Su25_Readify_Web.Controllers
{
    [Route("[Controller]")]
    public class BooksController : Controller
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
        public BooksController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri(GetGivenAPIBaseUrl());
        }
        [HttpGet("BookList")]
        public async Task<IActionResult> BookList(int page = 1, string searchTitle = null,
            List<int> cateIds = null, string orderBy = "Desc",bool isFree = false,string userId = null)
        {
            

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

            // Lấy danh sách các Book yêu thích từ API
            List<int> favoriteBookIds = new List<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                var favoriteResult = await GetApiDataAsync<JObject>($"api/Books/GetUserFavorites?userId={userId}");
                if (favoriteResult != null && favoriteResult["items"] != null)
                {
                    var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();
                    favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();
                }
            }
            // Gán IsFavorite
            foreach (var book in books)
            {
                book.IsFavorite = favoriteBookIds.Contains(book.Id);
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
        public async Task<IActionResult> BookDetails(string userId,int bookId, int pageNumber = 1)
        {

            var book =await GetApiDataAsync<Book>($"api/Books/GetBookById/{bookId}");
            if (book == null) return RedirectToAction("BookList", "Books");
            //Lấy tất cả comment của sách
            var commentResponse = await GetApiDataAsync<JObject>($"api/Comments/GetCommentsByBookId/{bookId}?pageNumber={pageNumber}&pageSize=5");

            var comments = commentResponse["comments"].ToObject<List<Comment>>();
            var totalComments = commentResponse["totalComments"].ToObject<int>();
            var pageSize = 5;
            var totalPage = (int)Math.Ceiling((double)totalComments / 5);
            //Lấy danh sách book liên quan tới tác giả
            var relatedBooks = await GetApiDataAsync<List<Book>>($"api/Books/RelatedBooks/{bookId}");
            // Lấy danh sách các Book yêu thích từ API
            List<int> favoriteBookIds = new List<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                var favoriteResult = await GetApiDataAsync<JObject>($"api/Books/GetUserFavorites?userId={userId}");
                if (favoriteResult != null && favoriteResult["items"] != null)
                {
                    var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();
                    favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();
                }
            }

            
            var isFavor = false;
            if (favoriteBookIds.Contains(bookId)) isFavor = true;

            var chapterQuan = book.Chapters.Count();
            var result = new BookDetailsViewModel
            {
                Book = book,
                ChapterQuantity = chapterQuan,
                isFavorite = isFavor,
                UserId = userId,
                RelatedBooks = relatedBooks,
                PagedComments = new PagedResult<Comment>
                {
                    Items = comments,
                    TotalItems = totalComments,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    TotalPage = totalPage
                }
            };
            return View(result);
        }
        [HttpGet("Read")]
        public async Task<IActionResult> Read(string userId, int bookId, int chapterOrder)
        {

            if (bookId == null && chapterOrder == null) return RedirectToAction("BookDetails", "Books");

            var book = await GetApiDataAsync<Book>($"api/Books/GetBookById/{bookId}");
            if (book == null ) return RedirectToAction("BookList", "Books");

            //Kiểm tra sách free hoặc đã mua
            if (book.IsFree == false) return RedirectToAction("BookList", "Books");

            //Lấy chapter
            var chapters = await GetApiDataAsync<List<Chapter>>($"api/Books/GetAllChapterByBookId/{bookId}");
            if(chapters == null) return RedirectToAction("BookDetails", "Books", new { bookId = bookId });

            int chapterId = chapters.FirstOrDefault(c => c.ChapterOrder == chapterOrder).Id;

            var query = await GetApiDataAsync<ReadViewModel>($"/api/Chapters/GetChapter?bookId={bookId}&chapterOrder={chapterOrder}");
            if (query == null) return RedirectToAction("BookDetails", "Books", new { bookId = bookId });
            
            var result = new ReadViewModel
            {
                Book = book,
                ChapterOrder = chapterOrder,
                Content = query.Content,
                Title = query.Title,
                Chapters = chapters
            };
            //Thêm vào danh sách đã đọc nếu người dùng đã đăng nhập
            if (userId != null)
            {
                var add = new RecentReadModel
                {
                    BookId = bookId,
                    UserId = userId,
                    ChapterId = chapterId
                };
                var addToRecent = await PostApiDataAsync<RecentReadModel>($"api/Books/AddToRecentRead", add);

            }
            return View(result);
        }
        [HttpGet("FavoritesList")]
        public async Task<IActionResult> FavoritesList(string userId,int page = 1, string searchTitle = null,
           List<int> cateIds = null, string orderBy = "Desc", bool isFree = false)
        {
            if (userId == null) return RedirectToAction("Index", "Home");
            var url = $"api/Books/GetUserFavorites?userId={userId}&page={page}&searchTitle={searchTitle}&orderBy={orderBy}&isFree={isFree}";
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

            // Lấy danh sách các Book yêu thích từ API
            var favoriteResult = await GetApiDataAsync<JObject>($"api/Books/GetUserFavorites?userId={userId}");
            var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();

            // Lấy danh sách Id của các Book yêu thích
            var favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();

            // Gán IsFavorite
            foreach (var book in books)
            {
                book.IsFavorite = favoriteBookIds.Contains(book.Id);
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
        [HttpGet("RecentList")]
        public async Task<IActionResult> RecentList(string userId,int page = 1, string searchTitle = null,
            List<int> cateIds = null, string orderBy = "Desc", bool isFree = false)
        {
            if (userId == null) return RedirectToAction("Login", "Auths");

            var url = $"api/Books/GetAllRecentRead?userId={userId}&page={page}&searchTitle={searchTitle}&orderBy={orderBy}&isFree={isFree}";
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

            // Lấy danh sách các Book yêu thích từ API
            var favoriteResult = await GetApiDataAsync<JObject>($"api/Books/GetUserFavorites?userId={userId}");
            var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();

            // Lấy danh sách Id của các Book yêu thích
            var favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();

            // Gán IsFavorite
            foreach (var book in books)
            {
                book.IsFavorite = favoriteBookIds.Contains(book.Id);
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
