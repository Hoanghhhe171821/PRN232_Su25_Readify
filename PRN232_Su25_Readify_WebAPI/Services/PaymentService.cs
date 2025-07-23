using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.TopUpCoints;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ReadifyDbContext _context;
        private readonly MoMoConfig _config;

        public PaymentService(IHttpClientFactory httpClientFactory, ReadifyDbContext context,
            IOptions<MoMoConfig> config)
        {
            _config = config.Value;
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task<TopUpResponse> CreateMoMoOrder(int points, string userId)
        {
            var amount = points * 1000; // 1 xu = 1000 VNĐ
            var orderId = Guid.NewGuid().ToString();
            var requestId = Guid.NewGuid().ToString();

            // Build rawData theo đúng format yêu cầu của MoMo
            string rawData = $"accessKey={_config.AccessKey}" +
                             $"&amount={amount}" +
                             $"&extraData=" +
                             $"&ipnUrl={_config.NotifyUrl}" +
                             $"&orderId={orderId}" +
                             $"&orderInfo=Nap {points} xu cho user {userId}" +
                             $"&partnerCode={_config.PartnerCode}" +
                             $"&redirectUrl={_config.ReturnUrl}" +
                             $"&requestId={requestId}" +
                             $"&requestType=captureWallet";

            // Tính chữ ký HMAC SHA256
            var encoding = Encoding.UTF8;
            using var hmac = new HMACSHA256(encoding.GetBytes(_config.SecretKey));
            var hash = hmac.ComputeHash(encoding.GetBytes(rawData));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            // Build body gửi tới MoMo
            var body = new
            {
                partnerCode = _config.PartnerCode,
                accessKey = _config.AccessKey,
                requestId = requestId,
                amount = amount.ToString(),
                orderId = orderId,
                orderInfo = $"Nap {points} xu cho user {userId}",
                redirectUrl = _config.ReturnUrl,
                ipnUrl = _config.NotifyUrl,
                extraData = "",
                requestType = "captureWallet",
                signature = signature,
                lang = "vi"
            };

            // Gửi POST đến MoMo
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(_config.Endpoint, body);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (json.TryGetProperty("payUrl", out var payUrlProp) &&
                json.TryGetProperty("deeplink", out var qrUrlProp))
            {
                var payUrl = payUrlProp.GetString();
                var qrUrl = qrUrlProp.GetString();

                // Lưu giao dịch vào DB
                var transaction = new TopUpTransaction
                {
                    UserId = userId,
                    Points = points,
                    Amount = amount,
                    MoMoOrderId = orderId,
                    MoMoRequestId = requestId,
                    QrCodeUrl = qrUrl ?? "",
                    PaymentUrl = payUrl ?? "",
                    Status = "PENDING"
                };

                _context.TopUpTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return new TopUpResponse
                {
                    TransactionId = transaction.Id,
                    QrCodeUrl = qrUrl ?? "",
                    PayUrl = payUrl ?? ""
                };
            }
            else
            {
                throw new Exception("Không nhận được payUrl hoặc deeplink từ MoMo.");
            }
        }


        public async Task<string> CheckMoMoTransaction(string orderId)
        {
            var tx = await _context.TopUpTransactions.FirstOrDefaultAsync(t => t.MoMoOrderId == orderId);
            return "SUCCESS";
        }

       
    }
}
