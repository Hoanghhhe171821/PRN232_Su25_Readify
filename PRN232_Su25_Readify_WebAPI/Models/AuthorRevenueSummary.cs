using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class AuthorRevenueSummary : BaseId
    {
        public int TotalRevenue { get; set; }
        public int TotalPaid { get; set; }
        public int TotalUnpaid => TotalRevenue - TotalPaid;

        
        public int AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public Author? Author { get; set; }
    }
}
