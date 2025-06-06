using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class Favorite
{
    public int UserId { get; set; }

    public int BookId { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
