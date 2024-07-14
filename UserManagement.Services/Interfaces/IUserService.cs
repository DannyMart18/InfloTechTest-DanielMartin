using System.Collections.Generic;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    IEnumerable<User> FilterByActive(bool isActive);
    IEnumerable<User> GetAll();
    User GetById(long id);
    void Create(User user);
    void Update(User user);
    void Delete(long id);

    void CreateLog(long userId, string action, string details);
    IEnumerable<Log> GetLogsForUser(long userId);
    IEnumerable<Log> GetAllLogs();
}
