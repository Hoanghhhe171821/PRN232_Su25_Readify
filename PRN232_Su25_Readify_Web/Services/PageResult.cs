using PRN232_Su25_Readify_Web.Models;

namespace PRN232_Su25_Readify_Web.Services
{
    public class PageResult
    {
        public static Pagination<T> ToPaginatedList<T>(List<T> source, int currentPage, int pageSize)
        {
            if (currentPage < 1) currentPage = 1;
            if (pageSize < 1) pageSize = 6;

            var totalCount = source.Count();
            var items = source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();


            return new Pagination<T>(items, totalCount, currentPage, pageSize);
        }
    }
}
