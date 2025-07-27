using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Reflection.Emit;

namespace PRN232_Su25_Readify_WebAPI.DbContext
{
    public class ReadifyDbContext : IdentityDbContext<AppUser>
    {
        public ReadifyDbContext(DbContextOptions<ReadifyDbContext> options) : base(options) { }


        public DbSet<AppUser> Users { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<AuthorRequest> AuthorRequests { get; set; }
        public DbSet<AuthorRevenueSummary> RevenueSummaries { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookRevenueSummary> BookRevenueSummaries { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ChapterError> ChaptersError { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<ErrorType> ErrorTypes { get; set; }
        public DbSet<Favorite> Favorite { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<RecentRead> RecentRead { get; set; }
        public DbSet<RoyaltyPayoutRequest> RoyaltyPayoutRequests { get; set; }
        public DbSet<RoyaltyPayoutTransaction> RoyaltyPayoutTransaction { get; set; }
        public DbSet<RoyaltyTransaction> RoyaltyTransaction { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<TopUpTransaction> TopUpTransactions { get; set; }
        public DbSet<AuthorRevenueSummary> AuthorRevenueSummary { get; set; }

        public DbSet<BookLicense> BookLicenses { get; set; }
        public DbSet<ContributorRequest> ContributorRequests { get; set; }

        public DbSet<RequestAuthor> RequestAuthors { get; set; }
        public DbSet<FeedbackRequest> FeedbackRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Cart>(entity =>
            {
                entity.ToTable("Carts");

                entity.HasKey(c => c.Id);

                entity.HasIndex(c => c.UserId).IsUnique(); // Mỗi User chỉ có 1 Cart

                entity.Property(c => c.CreateDate)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(c => c.AppUser)
                      .WithOne()
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<TopUpTransaction>()
               .HasOne(t => t.User)
               .WithMany() 
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");

                entity.HasKey(ci => ci.Id);

                entity.Property(ci => ci.CreateDate)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Book)
                      .WithMany()
                      .HasForeignKey(ci => ci.BookId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Đảm bảo không có trùng sách trong giỏ của 1 user
                entity.HasIndex(ci => new { ci.CartId, ci.BookId }).IsUnique();
            });
            builder.Entity<IdentityRole>().HasData(
             new IdentityRole
             {
                 Id = "1f42a404-6ac2-4a88-9a2c-e2f3f5b2b799",
                 Name = "Admin",
                 NormalizedName = "ADMIN",
                 ConcurrencyStamp = "7fba3f78-87d0-4728-91b1-21be2cc50238"
             },
             new IdentityRole
             {
                 Id = "b5870c27-8a70-4e35-bd40-5a3a6b410fa2",
                 Name = "User",
                 NormalizedName = "USER",
                 ConcurrencyStamp = "88a78940-1529-4686-b6d9-071e4ff703b0"
             },
               new IdentityRole
               {
                   Id = "c3dcae72-d23e-4cc7-91e5-fae9e1d10c1b",
                   Name = "Author",
                   NormalizedName = "AUTHOR",
                   ConcurrencyStamp = "de93198b-9a8a-4c74-a17b-08c34743c43e"
               }
          );

            builder.Entity<BookCategory>().HasKey(bc => new { bc.BookId, bc.CategoryId });
            builder.Entity<Favorite>().HasKey(f => new { f.BookId, f.UserId });
            builder.Entity<RecentRead>()
                .HasKey(r => r.Id);
            builder.Entity<RecentRead>()
                .HasIndex(r => new { r.UserId, r.BookId, r.ChapterId })
                .IsUnique();
            builder.Entity<AuthorRequest>().HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ với AppUser - admin duyệt
            builder.Entity<AuthorRequest>().HasOne(e => e.Admin)
                .WithMany()
                .HasForeignKey(e => e.ReviewBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<RoyaltyPayoutTransaction>(entity =>
            {
                entity.ToTable("RoyaltyPayoutTransaction");

                entity.HasKey(e => e.Id);

                // Tránh multiple cascade path bằng cách dùng Restrict
                entity.HasOne(e => e.RoyaltyTransaction)
                    .WithMany()
                    .HasForeignKey(e => e.RoyaltyTransactionId)
                    .OnDelete(DeleteBehavior.Restrict); // 

                entity.HasOne(e => e.RoyaltyPayoutRequest)
                    .WithMany(p => p.RoyaltyPayoutTransactions)
                    .HasForeignKey(e => e.RoyaltyPayoutRequestId)
                    .OnDelete(DeleteBehavior.Cascade); // Hoặc cũng có thể là Restrict nếu vẫn lỗi
            });
            builder.Entity<RoyaltyTransaction>(entity =>
            {
                entity.ToTable("RoyaltyTransaction");

                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.Amount).IsRequired();

                // Giữ Cascade nếu bạn muốn xóa Book sẽ xóa luôn các giao dịch (nếu thực sự cần)
                entity.HasOne(rt => rt.Book)
                    .WithMany(b => b.RoyaltyTransactions)
                    .HasForeignKey(rt => rt.BookId)
                    .OnDelete(DeleteBehavior.Restrict); //  Tránh multiple cascade

                entity.HasOne(rt => rt.Author)
                    .WithMany()
                    .HasForeignKey(rt => rt.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict); //  Cũng nên Restrict

                entity.HasOne(rt => rt.OrderItem)
                    .WithMany()
                    .HasForeignKey(rt => rt.OrderItemId)
                    .OnDelete(DeleteBehavior.Restrict); // 
            });

            // book license
            builder.Entity<BookLicense>(entity =>
            {
                entity.ToTable("BookLicenses");

                entity.HasKey(bl => bl.Id);

                // Quan hệ với AppUser (Cascade khi xóa User)
                entity.HasOne(bl => bl.User)
                      .WithMany(u => u.BookLicenses)
                      .HasForeignKey(bl => bl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ với Book (Cascade khi xóa Book)
                entity.HasOne(bl => bl.Book)
                      .WithMany(b => b.BookLicenses)
                      .HasForeignKey(bl => bl.BookId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ với OrderItem (KHÔNG cascade để tránh multiple paths)
                entity.HasOne(bl => bl.OrderItem)
                      .WithMany(oi => oi.BookLicenses)
                      .HasForeignKey(bl => bl.OrderItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ContributorRequest>()
                .Property(e => e.Status)
                .HasConversion<string>();

        }
    }
}
