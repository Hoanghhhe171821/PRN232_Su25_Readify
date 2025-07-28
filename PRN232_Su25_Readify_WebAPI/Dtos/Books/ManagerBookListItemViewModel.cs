namespace PRN232_Su25_Readify_WebAPI.Dtos.Books
{
    public class ManagerBookListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
    }
}