using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class Chapter
{
    public int ChapterId { get; set; }

    public int? BookId { get; set; }

    public string? Title { get; set; }

    public string? FilePath { get; set; }

    public int? ChapterOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Book? Book { get; set; }

    public virtual ICollection<Error> Errors { get; set; } = new List<Error>();
}
