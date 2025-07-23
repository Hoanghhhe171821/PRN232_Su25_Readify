namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class MoMoConfig
    {
        public string PartnerCode { get; set; } = null!;
        public string AccessKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string Endpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
        public string ReturnUrl { get; set; } = null!;
        public string NotifyUrl { get; set; } = null!;
    }
}
