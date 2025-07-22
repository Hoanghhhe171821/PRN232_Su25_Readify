using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos.Books
{
    public class ReadViewModel
    {
        public Book Book{ get; set; }
        public List<Chapter> Chapters { get; set; }
        public int ChapterOrder { get; set; }
        public string Title { get; set; }
        public string PdfPath { get; set; }
    }
}
