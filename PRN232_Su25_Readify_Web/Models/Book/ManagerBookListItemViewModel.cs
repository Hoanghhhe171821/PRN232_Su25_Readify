namespace PRN232_Su25_Readify_Web.Models.Book
{
    public class ManagerBookListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
    }
}