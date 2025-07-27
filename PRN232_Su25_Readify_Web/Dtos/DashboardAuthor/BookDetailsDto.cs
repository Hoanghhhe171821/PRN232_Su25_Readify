using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_Web.Dtos.DashboardAuthor
{
    public class BookDetailsDto
    {
        public Book Book { get; set; }
        public List<Chapter> Chapters {get;set;}
    }
}
