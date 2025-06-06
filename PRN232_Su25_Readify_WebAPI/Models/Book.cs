using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? AuthorId { get; set; }

    public bool? IsFree { get; set; }

    public int? Price { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UploadedBy { get; set; }

    public virtual Author? Author { get; set; }

    public virtual BookRevenue? BookRevenue { get; set; }

    public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Error> Errors { get; set; } = new List<Error>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<RecentRead> RecentReads { get; set; } = new List<RecentRead>();

    public virtual User? UploadedByNavigation { get; set; }
}
