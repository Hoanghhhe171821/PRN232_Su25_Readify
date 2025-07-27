using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class RequestAuthor : BaseId
    {
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public string FullName { get; set; }
        public string CCCD { get; set; }
        public DateTime BirthDate { get; set; }
        public string Reason { get; set; }
        public string BankAccount { get; set; }
        public int IsApproved { get; set; } = 0;
    }
}
