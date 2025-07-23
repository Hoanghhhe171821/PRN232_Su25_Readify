using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Models;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface ICartService
    {
        Task<PagedResult<Book>> GetAllCart(int page = 1,int pageSize = 10);
        Task<string> AddCartItem(CartItem item);
        Task<string> RemoveCartItem(int cartItemId);
        Task<bool> RemoveAllCartItem();
    }
}
