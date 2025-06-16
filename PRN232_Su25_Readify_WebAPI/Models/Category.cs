namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class Category : BaseId
    {
        public string Name { get; set; }

        public ICollection<BookCategory>? BookCategories { get; set; }
    }
}
