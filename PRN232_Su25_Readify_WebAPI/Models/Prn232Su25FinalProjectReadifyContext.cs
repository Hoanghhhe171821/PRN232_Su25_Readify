using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class Prn232Su25FinalProjectReadifyContext : DbContext
{
    public Prn232Su25FinalProjectReadifyContext()
    {
    }

    public Prn232Su25FinalProjectReadifyContext(DbContextOptions<Prn232Su25FinalProjectReadifyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookRevenue> BookRevenues { get; set; }

    public virtual DbSet<Chapter> Chapters { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Error> Errors { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<RecentRead> RecentReads { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleRequest> RoleRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Warning> Warnings { get; set; }

    public virtual DbSet<WithdrawRequest> WithdrawRequests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyCnn"));

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Authors__70DAFC34184D049B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Website).HasMaxLength(255);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C207335C0672");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsFree).HasDefaultValue(false);
            entity.Property(e => e.Price).HasDefaultValue(0);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_Books_AuthorId_Authors");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Books)
                .HasForeignKey(d => d.UploadedBy)
                .HasConstraintName("FK_Books_UploadedBy");
        });

        modelBuilder.Entity<BookRevenue>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__BookReve__3DE0C207DA8EA901");

            entity.ToTable("BookRevenue");

            entity.Property(e => e.BookId).ValueGeneratedNever();
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalRevenue).HasDefaultValue(0);

            entity.HasOne(d => d.Book).WithOne(p => p.BookRevenue)
                .HasForeignKey<BookRevenue>(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookReven__BookI__619B8048");
        });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(e => e.ChapterId).HasName("PK__Chapters__0893A36AFB1C86D3");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Book).WithMany(p => p.Chapters)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__Chapters__BookId__3A81B327");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA42246F1F");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.Comments)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__Comments__BookId__52593CB8");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Comments__UserId__5165187F");
        });

        modelBuilder.Entity<Error>(entity =>
        {
            entity.HasKey(e => e.ErrorId).HasName("PK__Errors__35856A2A19301148");

            entity.Property(e => e.ErrorType).HasMaxLength(50);
            entity.Property(e => e.ReportedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.Errors)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__Errors__BookId__5BE2A6F2");

            entity.HasOne(d => d.Chapter).WithMany(p => p.Errors)
                .HasForeignKey(d => d.ChapterId)
                .HasConstraintName("FK__Errors__ChapterI__5CD6CB2B");

            entity.HasOne(d => d.Reporter).WithMany(p => p.Errors)
                .HasForeignKey(d => d.ReporterId)
                .HasConstraintName("FK__Errors__Reporter__5AEE82B9");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.BookId }).HasName("PK__Favorite__7456C06CBDD35B40");

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Favorites__BookI__4D94879B");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Favorites__UserI__4CA06362");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A380947A804");

            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Success");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Payments__UserId__440B1D61");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("PK__Purchase__6B0A6BBEF8DA14D7");

            entity.Property(e => e.PurchaseDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__Purchases__BookI__3F466844");

            entity.HasOne(d => d.User).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Purchases__UserI__3E52440B");
        });

        modelBuilder.Entity<RecentRead>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.BookId }).HasName("PK__RecentRe__7456C06CF10D0A7D");

            entity.Property(e => e.ReadAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Book).WithMany(p => p.RecentReads)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RecentRea__BookI__571DF1D5");

            entity.HasOne(d => d.User).WithMany(p => p.RecentReads)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RecentRea__UserI__5629CD9C");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A244AEE4A");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616072AFAAB4").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<RoleRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__RoleRequ__33A8517A8D52996A");

            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RoleRequested).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.User).WithMany(p => p.RoleRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__RoleReque__UserI__6B24EA82");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C199A8A90");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E412B55D6D").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534E5BF62B4").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Points).HasDefaultValue(0);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760AD6071B4D1");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRoles__RoleI__300424B4");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRoles__UserI__2F10007B");
        });

        modelBuilder.Entity<Warning>(entity =>
        {
            entity.HasKey(e => e.WarningId).HasName("PK__Warnings__2145715868B74913");

            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Receiver).WithMany(p => p.WarningReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK__Warnings__Receiv__66603565");

            entity.HasOne(d => d.Sender).WithMany(p => p.WarningSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__Warnings__Sender__656C112C");
        });

        modelBuilder.Entity<WithdrawRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Withdraw__33A8517AB417BF18");

            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.User).WithMany(p => p.WithdrawRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__WithdrawR__UserI__48CFD27E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
