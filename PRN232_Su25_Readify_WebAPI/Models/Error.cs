using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class Error
{
    public int ErrorId { get; set; }

    public int? ReporterId { get; set; }

    public int? BookId { get; set; }

    public int? ChapterId { get; set; }

    public string? ErrorType { get; set; }

    public string? Description { get; set; }

    public DateTime? ReportedAt { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Chapter? Chapter { get; set; }

    public virtual User? Reporter { get; set; }
}
