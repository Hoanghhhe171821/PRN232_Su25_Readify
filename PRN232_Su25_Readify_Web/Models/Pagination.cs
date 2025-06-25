namespace PRN232_Su25_Readify_Web.Models
{
    public class Pagination<T>
    {
        // Danh sách dữ liệu trên trang hiện tại
        public List<T> Items { get; private set; }

        // Số trang hiện tại
        public int CurrentPage { get; private set; }

        // Tổng số phần tử trong toàn bộ kết quả (sau khi lọc điều kiện)
        public int TotalCount { get; private set; }

        // Kích thước trang (số phần tử trên mỗi trang)
        public int PageSize { get; private set; }

        // Tổng số trang (tính từ TotalCount và PageSize)
        public int TotalPages { get; private set; }

        // Kiểm tra xem có trang trước đó không
        public bool HasPreviousPage => CurrentPage > 1;

        // Kiểm tra xem có trang tiếp theo không
        public bool HasNextPage => CurrentPage < TotalPages;

        // Constructor
        public Pagination(List<T> items, int totalCount, int currentPage, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

    }
}
