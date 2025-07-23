using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PRN232_Su25_Readify_Web.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Dtos.Books;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using static System.Reflection.Metadata.BlobBuilder;

namespace PRN232_Su25_Readify_Web.Controllers
{
    [Route("[Controller]")]
    public class BooksController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _uri = "https://localhost:7267/";

        private string GetGivenAPIBaseUrl()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            string baseUrl = config["GivenAPIBaseUrl"];
            return baseUrl;
        }
        public BooksController(IHttpClientFactory factory, IMemoryCache cache, IWebHostEnvironment env)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri(GetGivenAPIBaseUrl());
            _cache = cache;
            _env = env;
            _httpClientFactory = factory;
        }
        [HttpGet("BookList")]
        public async Task<IActionResult> BookList(int page = 1, string searchOption = null, string searchBy = null,
            List<int> cateIds = null, string orderBy = "Desc", bool isFree = false)
        {


            var url = $"api/Books/GetAllBooks?page={page}&searchBy={searchBy}&searchOption={searchOption}&orderBy={orderBy}&isFree={isFree}";
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
            //Get Author By API
            var authors = await GetApiDataAsync<List<Author>>("api/Authors/GetAllAuthors");
            // Lấy danh sách các Book yêu thích từ API
            List<int> favoriteBookIds = new List<int>();


                var favoriteResult = await GetAuthorizedApiDataAsync<JObject>($"api/Books/GetUserFavorites?");
                if (favoriteResult != null && favoriteResult["items"] != null)
                {
                    var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();
                    favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();
                }

            // Gán IsFavorite
            foreach (var book in books)
            {
                book.IsFavorite = favoriteBookIds.Contains(book.Id);
                //Kiểm tra sách đã mua
                var isLicense = await GetAuthorizedApiDataAsync<bool>($"api/Books/checkBookLicence/{book.Id}");
                if (isLicense) book.IsLicense = true;

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
                Authors = authors.ToList(),
                OrderBy = orderBy,
                SearchBy = searchBy,
                SearchOption = searchOption,
                IsFree = isFree
            };
            return View(model);
        }
        [HttpGet("BookDetails/{bookId}")]
        public async Task<IActionResult> BookDetails(int bookId, int pageNumber = 1)
        {

            var book = await GetApiDataAsync<Book>($"api/Books/GetBookById/{bookId}");
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

            var favoriteResult = await GetAuthorizedApiDataAsync<JObject>($"api/Books/GetUserFavorites");
            if (favoriteResult != null && favoriteResult["items"] != null)
            {
                var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();
                favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();
            }


            var isFavor = false;
            if (favoriteBookIds.Contains(bookId)) isFavor = true;
            var chapterQuan = book.Chapters.Count();

            // Lấy danh sách chương đã đọc
            List<int> chapterIds = new List<int>();
            var lastedRead = new RecentedReadChapters();

                var recentedReadChapters = await GetAuthorizedApiDataAsync<List<RecentedReadChapters>>($"api/Chapters/GetRecentedReadChapters?bookId={bookId}");
                foreach (var recent in recentedReadChapters)
                {
                    chapterIds.Add(recent.ChapterId);
                }
                lastedRead = recentedReadChapters.OrderByDescending(rd => rd.DateRead).FirstOrDefault();

            // Chuyển Book.Chapters thành ChapterDto có đánh dấu isRead
            var chapterDtos = book.Chapters
                .OrderBy(c => c.ChapterOrder)
                .Select(c => new ChapterDto
                {
                    Chapter = c,
                    isRead = chapterIds.Contains(c.Id)
                })
                .ToList();
            //Kiểm tra sách đã mua
            var isLicense = await GetAuthorizedApiDataAsync<bool>($"api/Books/checkBookLicence/{bookId}");

            var result = new BookDetailsViewModel
            {
                Book = book,
                ChapterQuantity = chapterQuan,
                isFavorite = isFavor,
                RelatedBooks = relatedBooks,
                ChapterDto = chapterDtos,
                LastRead = lastedRead,
                IsLicensed = isLicense,
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
        public async Task<IActionResult> Read(int bookId, int chapterOrder)
        {

            if (bookId == null && chapterOrder == null) return RedirectToAction("BookDetails", "Books");

            var book = await GetApiDataAsync<Book>($"api/Books/GetBookById/{bookId}");
            if (book == null) return RedirectToAction("BookList", "Books");
            var isLicensed = await GetAuthorizedApiDataAsync<bool>($"api/Books/checkBookLicence/{bookId}");
            //Kiểm tra sách free hoặc đã mua
            if (book.IsFree == false && !isLicensed) return RedirectToAction("BookList", "Books");

            //Lấy chapter
            var chapters = await GetApiDataAsync<List<Chapter>>($"api/Books/GetAllChapterByBookId/{bookId}");
            if (chapters == null) return RedirectToAction("BookDetails", "Books", new { bookId = bookId});

            var chapter = chapters.FirstOrDefault(c => c.ChapterOrder == chapterOrder);
            if (chapter == null) return RedirectToAction("BookDetails", "Books", new { bookId = bookId});
            int chapterId = chapter.Id;

            var safeTitle = ToSafeFileName(book.Title);
            var fileName = $"{safeTitle}_Chapter_{chapterOrder}.pdf";
            var cacheKey = $"Pdf_{bookId}_{chapterOrder}"; //Khóa cho IMemoryCache
            var tempPath = Path.Combine(_env.WebRootPath, "temp", fileName);

            byte[] fileBytes;
            //Kiểm tra IMemoryCache
            if (_cache.TryGetValue(cacheKey, out fileBytes))
            {
                // File đã có trong cache, sử dụng nó
            }
            //Khôgn có trong cache, kiểm tra wwwroot/temp
            else if (System.IO.File.Exists(tempPath))
            {
                fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
                //Lưu vào cache 
                _cache.Set(cacheKey, fileBytes, TimeSpan.FromMinutes(30));
            }
            else
            {
                var response = await _httpClient.GetAsync($"/api/Chapters/GetChapter?bookId={bookId}&chapterOrder={chapterOrder}");
                if (!response.IsSuccessStatusCode)
                    return RedirectToAction("BookDetails", "Books", new { bookId });

                fileBytes = await response.Content.ReadAsByteArrayAsync();
                // Lưu vào thư mục temp
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);
                await System.IO.File.WriteAllBytesAsync(tempPath, fileBytes);

                // Lưu vào cache
                _cache.Set(cacheKey, fileBytes, TimeSpan.FromMinutes(30)); // Đặt thời gian hết hạn 30 phút
                //Đặt hẹn xóa file sau 30 phút (nếu vẫn tồn tại)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(30));
                    if (System.IO.File.Exists(tempPath))
                    {
                        try { System.IO.File.Delete(tempPath); } catch { }
                    }
                });
            }

            var result = new ReadViewModel
            {
                Book = book,
                ChapterOrder = chapterOrder,
                Title = chapter.Title,
                Chapters = chapters,
                PdfPath = $"/temp/{fileName}"
            };
            //Thêm vào danh sách đã đọc nếu người dùng đã đăng nhập
                var add = new RecentReadModel
                {
                    BookId = bookId,
                    ChapterId = chapterId
                };
                var addToRecent = await PostAuthorizedApiDataAsync<RecentReadModel>($"api/Books/AddToRecentRead", add);

            
            return View(result);
        }
        [HttpGet("FavoritesList")]
        public async Task<IActionResult> FavoritesList( int page = 1, string searchOption = null, string searchBy = null,
           List<int> cateIds = null, string orderBy = "Desc", bool isFree = false)
        {
            var url = $"api/Books/GetUserFavorites?page={page}&searchBy={searchBy}&searchOption={searchOption}&orderBy={orderBy}&isFree={isFree}";
            if (cateIds != null && cateIds.Any())
            {
                url += "&" + string.Join("&", cateIds.Select(id => $"cateIds={id}"));
            }

            //Get Book by API
            var booksJsonResult = await GetAuthorizedApiDataAsync<JObject>(url);
            if (booksJsonResult == null) return RedirectToAction("Login", "Auths");
            var books = booksJsonResult["items"].ToObject<List<BookViewModel>>();
            var totalItems = booksJsonResult["totalItems"].ToObject<int>();
            var pageSize = booksJsonResult["pageSize"].ToObject<int>();
            var totalPage = (int)Math.Ceiling((double)totalItems / pageSize);

            //Get Cate by API
            var categories = await GetApiDataAsync<List<Category>>("api/Categories/GetAllCategories");
            //Get Author By API
            var authors = await GetApiDataAsync<List<Author>>("api/Authors/GetAllAuthors");
            // Lấy danh sách các Book yêu thích từ API
            var favoriteResult = await GetAuthorizedApiDataAsync<JObject>($"api/Books/GetUserFavorites?");
            var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();

            // Lấy danh sách Id của các Book yêu thích
            var favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();

            // Gán IsFavorite
            foreach (var book in books)
            {
                book.IsFavorite = favoriteBookIds.Contains(book.Id);
                //Kiểm tra sách đã mua
                var isLicense = await GetAuthorizedApiDataAsync<bool>($"api/Books/checkBookLicence/{book.Id}");
                if (isLicense) book.IsLicense = true;
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
                Authors = authors.ToList(),
                SearchBy = searchBy,
                SearchOption = searchOption,
                IsFree = isFree
            };
            return View(model);
        }
        [HttpGet("RecentList")]
        public async Task<IActionResult> RecentList( int page = 1, string searchOption = null, string searchBy = null,
            List<int> cateIds = null, string orderBy = "Desc", bool isFree = false)
        {

            var url = $"api/Books/GetAllRecentRead?page={page}&searchBy={searchBy}&searchOption={searchOption}&orderBy={orderBy}&isFree={isFree}";
            if (cateIds != null && cateIds.Any())
            {
                url += "&" + string.Join("&", cateIds.Select(id => $"cateIds={id}"));
            }

            //Get Book by API
            var booksJsonResult = await GetAuthorizedApiDataAsync<JObject>(url);
            if (booksJsonResult == null) return RedirectToAction( "Login", "Auths");
            var books = booksJsonResult["items"].ToObject<List<BookViewModel>>();
            var totalItems = booksJsonResult["totalItems"].ToObject<int>();
            var pageSize = booksJsonResult["pageSize"].ToObject<int>();
            var totalPage = (int)Math.Ceiling((double)totalItems / pageSize);

            //Get Cate by API
            var categories = await GetApiDataAsync<List<Category>>("api/Categories/GetAllCategories");
            //Get Author By API
            var authors = await GetApiDataAsync<List<Author>>("api/Authors/GetAllAuthors");
            // Lấy danh sách các Book yêu thích từ API
            var favoriteResult = await GetAuthorizedApiDataAsync<JObject>($"api/Books/GetUserFavorites?");
            var favoriteBooks = favoriteResult["items"].ToObject<List<BookViewModel>>();

            // Lấy danh sách Id của các Book yêu thích
            var favoriteBookIds = favoriteBooks.Select(b => b.Id).ToList();

            // Gán IsFavorite
            foreach (var book in books)
            {
                book.IsFavorite = favoriteBookIds.Contains(book.Id);
                //Kiểm tra sách đã mua
                var isLicense = await GetAuthorizedApiDataAsync<bool>($"api/Books/checkBookLicence/{book.Id}");
                if (isLicense) book.IsLicense = true;
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
                Authors = authors.ToList(),
                OrderBy = orderBy,
                SearchBy = searchBy,
                SearchOption = searchOption,
                IsFree = isFree
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
        private async Task<T> GetAuthorizedApiDataAsync<T>(string apiUrl)
        {
            string fullUrl = _uri + apiUrl;
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
        private async Task<T> PostAuthorizedApiDataAsync<T>(string apiUrl, object body)
        {
            string fullUrl = _uri + apiUrl;
            var token = Request.Cookies["access_Token"];
            if (string.IsNullOrEmpty(token)) return default;

            var json = JsonConvert.SerializeObject(body);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(fullUrl, contentData);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            var resultData = JsonConvert.DeserializeObject<T>(resultJson);

            return resultData;
        }
        private static string ToSafeFileName(string title)
        {
            var normalized = title.Normalize(NormalizationForm.FormD);
            var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            var clean = new string(chars.ToArray());
            clean = Regex.Replace(clean, @"[^a-zA-Z0-9_\- ]", ""); // loại bỏ ký tự đặc biệt
            return clean.Replace(" ", "_"); // thay khoảng trắng bằng _
        }
    }
}
