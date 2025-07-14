using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public string SessionId { get; set; }            //  Mã phiên duy nhất (ghi nhận ở client và gửi lên)
        public string UserAgent { get; set; }            //  Chuỗi User-Agent từ trình duyệt hoặc thiết bị


        [ForeignKey("UserId")]
        public AppUser User { get; set; }
    }
}
