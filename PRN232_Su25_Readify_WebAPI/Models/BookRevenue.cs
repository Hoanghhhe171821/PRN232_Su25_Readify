using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class BookRevenue
{
    public int BookId { get; set; }

    public int? TotalRevenue { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Book Book { get; set; } = null!;
}
