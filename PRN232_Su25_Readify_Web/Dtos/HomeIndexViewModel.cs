using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos
{
    public class HomeIndexViewModel
    {
        public List<Book> RecommendBooks{ get; set; }
        public List<Book> NewReleaseBooks { get; set; }

    }
}
