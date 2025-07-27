namespace PRN232_Su25_Readify_WebAPI.Dtos.Books
{
    public class CreateBookWithFile
    {
        public IFormFile ImageFile { get; set; }
        public string BookData { get; set; }
    }
}
