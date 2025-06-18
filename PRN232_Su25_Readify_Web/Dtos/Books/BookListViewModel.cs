using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos.Books
{
    public class BookListViewModel
    {
        public PagedResult<Book> PagedBooks { get; set; }
        public List<Category> Categories { get; set; }
        public string OrderBy { get; set; }
        public string SearchTitle { get; set; }
        public bool IsFree { get; set; }
    }
}
