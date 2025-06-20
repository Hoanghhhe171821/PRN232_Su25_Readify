using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos.Books
{
    public class BookDetailsViewModel
    {
        public string UserId { get; set; }
        public Book Book{ get; set; }
        public int ChapterQuantity { get; set; }
        public bool isFavorite { get; set; }
    }
}
