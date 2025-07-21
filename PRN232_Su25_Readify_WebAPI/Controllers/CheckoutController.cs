using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var result = await _checkoutService.Checkout();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
