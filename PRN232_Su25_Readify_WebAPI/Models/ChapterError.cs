using PRN232_Su25_Readify_WebAPI.Models.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class ChapterError : BaseId
    {
        public string Description { get; set; }
        public ErrorStatus Status { get; set; } = ErrorStatus.Pending;

        public int ErrorTypeId { get; set; }
        public ErrorType? ErrorType { get; set; }

        public int ChapterId { get; set; }
        public Chapter? Chapter { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

    }
}
