using System;

namespace UserManagement.Models;

public class Log
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }

    // Navigation property
    public User? User { get; set; }
}
