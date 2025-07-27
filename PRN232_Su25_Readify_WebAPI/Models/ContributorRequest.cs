using PRN232_Su25_Readify_WebAPI.Models.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class ContributorRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Required]
        public string CitizenId { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string BankAccount { get; set; }

        [Required]
        public bool IsAgreedToPolicy { get; set; }

        [Required]
        public ContributorRequestStatus Status { get; set; } = ContributorRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public AppUser User { get; set; }
    }
}
