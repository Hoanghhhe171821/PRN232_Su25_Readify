using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos.Books
{
    public class BookListViewModel
    {
        public PagedResult<BookViewModel> PagedBooks { get; set; }
        public List<Category> Categories { get; set; }
        public List<Author> Authors { get; set; }
        public string OrderBy { get; set; }
        public string SearchBy { get; set; }
        public string SearchOption { get; set; }
        public bool IsFree { get; set; }
    }
    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public List<BookCategory> BookCategories { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsLicense { get; set; } = false;
    }

}
