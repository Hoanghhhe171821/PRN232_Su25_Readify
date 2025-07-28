namespace PRN232_Su25_Readify_Web.Models.Book
{
    public class ManagerBookReviewViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsActive { get; set; }
    }
}