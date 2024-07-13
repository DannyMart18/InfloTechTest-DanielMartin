using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Exceptions;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    public UserService(IDataContext dataAccess) => _dataAccess = dataAccess;

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public IEnumerable<User> FilterByActive(bool isActive)
    {
        return _dataAccess.GetAll<User>().Where(u => u.IsActive == isActive);
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    public User GetById(long id)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            throw new UserNotFoundException($"User with ID {id} not found.");
        }
        return user;
    }
    public void Create(User user)
    {
        _dataAccess.Create(user);
    }

    public void Update(User user)
    {
        _dataAccess.Update(user);
    }

    public void Delete(long id)
    {
        var user = GetById(id);
        if (user != null)
        {
            _dataAccess.Delete(user);
        }
    }
}
