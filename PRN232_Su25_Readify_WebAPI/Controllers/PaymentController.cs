using Microsoft.AspNetCore.Mvc;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Dtos.TopUpCoints;
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
        private readonly ITopUpTransService _topUpTransService;

        public PaymentController(IPaymentService payment, IAuthService authService,
            ReadifyDbContext context, ITopUpTransService topUpTransService)
        {
            _authService = authService;
            _payment = payment;
            _context = context;
            _topUpTransService = topUpTransService;
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
            return Ok(new
            {
                QrCodeUrl = tx.QrCodeUrl,
                PayUrl = tx.PaymentUrl,
                TransactionId = tx.Id,
                Status = tx.Status
            });
        }

        [HttpGet("TopUp/History")]
        public async Task<IActionResult> GetTopUpHistory(int pageIndex = 1, int pageSize = 5, string? userName = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            var result = await _topUpTransService.GetTopUpTransactionsAsync(pageIndex, pageSize, userName);
            return Ok(result);

        }

        [HttpGet("TopUp/getAllPayments")]
        public async Task<IActionResult> GetAllPayments()
        {
            return Ok(await _payment.FindAllPaymentHistory());
        }

        [HttpPut("TopUp/disbursePayment")]
        public async Task<IActionResult> DisbursePayment([FromBody] DisbursePaymentRequest request)
        {
            var response = await _payment.DisbursePayment(request.TopUpTransactionId, request.Status);
            if (request.Status == "SUCCESS")
            {
                await _authService.TopUpCoints(response.Points, response.UserId);
            }
            return Ok(response);
        }

    }
}
