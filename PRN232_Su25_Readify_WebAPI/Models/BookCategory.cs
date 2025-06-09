namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
