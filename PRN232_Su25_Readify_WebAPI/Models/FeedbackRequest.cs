using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class FeedbackRequest : BaseId
    {
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        [ForeignKey("ResponseUser")]
        public string? ResponseUserId { get; set; } = null;
        public AppUser? ResponseUser { get; set; } = null;
        public string? Response { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

    }
}
