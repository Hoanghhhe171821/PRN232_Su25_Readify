using Microsoft.AspNetCore.Identity;

namespace PRN232_Su25_Readify_WebAPI.Models
{
    public class AppUser : IdentityUser
    {
        public int Points { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<AuthorRequest>? AuthorRequests { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
        public ICollection<RecentRead>? RecentRead { get; set; }
        public ICollection<TopUpTransaction> TopUpTransactions { get; set; } = new List<TopUpTransaction>();

    }
}
