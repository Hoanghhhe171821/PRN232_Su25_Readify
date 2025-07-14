using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.Dtos.Cart;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10) {
            var cartList = await _cartService.GetAllCart(page,pageSize);
            return Ok(cartList);
        }

        [HttpPost]
        public async Task<IActionResult> AddCartItem([FromBody] CartAddDto item)
        {
            var cartDto = new CartItem { BookId = item.BookId };
            var success = await _cartService.AddCartItem(cartDto);
            return Ok(success);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var deleteSuccess = await _cartService.RemoveCartItem(id);
            return Ok(deleteSuccess);
        }
    }
}
