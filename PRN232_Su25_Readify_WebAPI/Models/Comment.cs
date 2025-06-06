using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? UserId { get; set; }

    public int? BookId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Book? Book { get; set; }

    public virtual User? User { get; set; }
}
