using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class Warning
{
    public int WarningId { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Content { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual User? Receiver { get; set; }

    public virtual User? Sender { get; set; }
}
