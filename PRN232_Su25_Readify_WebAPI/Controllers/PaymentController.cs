using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _payment;
        private readonly IAuthService _authService;
        private readonly ReadifyDbContext _context;

        public PaymentController(IPaymentService payment, IAuthService authService,
            ReadifyDbContext context)
        {
            _authService = authService;
            _payment = payment;
            _context = context;
        }

        [HttpPost("TopUp")]
        public async Task<IActionResult> TopUpCoins([FromBody] TopUpRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var response = await _payment.CreateMoMoOrder(request.Points, userId);
            return Ok(response);
        }

        [HttpGet("TopUp/{transactionId}")]
        public async Task<IActionResult> CheckTopUp(int transactionId)
        {
            var tx = await _context.TopUpTransactions.FindAsync(transactionId);
            if (tx == null) return NotFound();

            if (tx.Status == "SUCCESS")
                return Ok(new { Status = tx.Status });

            var status = await _payment.CheckMoMoTransaction(tx.MoMoOrderId);
            tx.Status = status;

            if (status == "SUCCESS")
            {
                await _authService.TopUpCoints(tx.Points, tx.UserId);
            }

            await _context.SaveChangesAsync();
            return Ok(new {
                QrCodeUrl = tx.QrCodeUrl,
                PayUrl = tx.PaymentUrl,
                TransactionId = tx.Id,
                Status = tx.Status
            });
        }

    }
}
