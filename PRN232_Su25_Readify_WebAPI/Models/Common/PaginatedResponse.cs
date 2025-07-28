namespace PRN232_Su25_Readify_WebAPI.Models.Common
{
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
    }
}