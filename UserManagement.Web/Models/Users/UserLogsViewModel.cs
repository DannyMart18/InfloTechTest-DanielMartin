using UserManagement.Models;

namespace UserManagement.Web.Models.Users;

public class UserLogsViewModel
{
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public List<Log> Logs { get; set; } = new List<Log>();  // Initialize with an empty list
}
