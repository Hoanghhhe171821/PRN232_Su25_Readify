using System;
using System.Collections.Generic;

namespace PRN232_Su25_Readify_WebAPI.Models;

public partial class RoleRequest
{
    public int RequestId { get; set; }

    public int? UserId { get; set; }

    public string? RoleRequested { get; set; }

    public string? Status { get; set; }

    public DateTime? RequestedAt { get; set; }

    public virtual User? User { get; set; }
}
