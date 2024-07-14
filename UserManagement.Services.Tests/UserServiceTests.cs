using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Exceptions;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public void GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetAll();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeSameAs(users);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FilterByActive_WhenCalled_ReturnsCorrectUsers(bool isActive)
    {
        // Arrange
        var service = CreateService();
        var users = SetupMixedUsers();

        // Act
        var result = service.FilterByActive(isActive);

        // Assert
        result.Should().AllSatisfy(user => user.IsActive.Should().Be(isActive));
        result.Should().BeEquivalentTo(users.Where(u => u.IsActive == isActive));
    }

    [Fact]
    public void FilterByActive_WhenNoUsersMatch_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService();
        SetupAllInactiveUsers();

        // Act
        var result = service.FilterByActive(true);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_WhenContextThrowsException_ShouldThrowException()
    {
        // Arrange
        _dataContext
            .Setup(s => s.GetAll<User>())
            .Throws(new Exception("Database error"));

        var service = CreateService();

        // Act & Assert
        service.Invoking(s => s.GetAll())
            .Should().Throw<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public void GetAll_ShouldIncludeDateOfBirth()
    {
        // Arrange
        var service = CreateService();
        var expectedDate = new DateTime(1990, 1, 1);
        SetupUsers(dateOfBirth: expectedDate);

        // Act
        var result = service.GetAll();

        // Assert
        result.Should().ContainSingle();
        result.First().DateOfBirth.Should().Be(expectedDate);
    }

    private IQueryable<User> SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true, DateTime? dateOfBirth = null)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive,
                DateOfBirth = dateOfBirth ?? DateTime.Now.AddYears(-30)
            }
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    private IQueryable<User> SetupMixedUsers()
    {
        var users = new[]
        {
            new User { Forename = "Active", Surname = "User", Email = "active@example.com", IsActive = true, DateOfBirth = DateTime.Now.AddYears(-30) },
            new User { Forename = "Inactive", Surname = "User", Email = "inactive@example.com", IsActive = false, DateOfBirth = DateTime.Now.AddYears(-30) },
            new User { Forename = "Another", Surname = "Active", Email = "another@example.com", IsActive = true, DateOfBirth = DateTime.Now.AddYears(-30) }
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    private IQueryable<User> SetupAllInactiveUsers()
    {
        var users = new[]
        {
            new User { Forename = "Inactive", Surname = "User1", Email = "inactive1@example.com", IsActive = false, DateOfBirth = DateTime.Now.AddYears(-30) },
            new User { Forename = "Inactive", Surname = "User2", Email = "inactive2@example.com", IsActive = false, DateOfBirth = DateTime.Now.AddYears(-30) },
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    [Fact]
    public void Create_WhenValidUser_ShouldCallDataContextCreate()
    {
        // Arrange
        var service = CreateService();
        var user = new User { Forename = "New", Surname = "User", Email = "newuser@example.com", DateOfBirth = DateTime.Now.AddYears(-25), IsActive = true };

        // Act
        service.Create(user);

        // Assert
        _dataContext.Verify(dc => dc.Create(It.Is<User>(u => u.Email == user.Email)), Times.Once);
    }

    [Fact]
    public void GetById_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var service = CreateService();
        var expectedUser = new User { Id = 1, Forename = "Existing", Surname = "User", Email = "existing@example.com", DateOfBirth = DateTime.Now.AddYears(-30), IsActive = true };
        _dataContext.Setup(dc => dc.GetAll<User>()).Returns(new[] { expectedUser }.AsQueryable());

        // Act
        var result = service.GetById(1);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public void GetById_WhenUserDoesNotExist_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var service = CreateService();
        _dataContext.Setup(dc => dc.GetAll<User>()).Returns(new User[] { }.AsQueryable());

        // Act & Assert
        service.Invoking(s => s.GetById(1))
            .Should().Throw<UserNotFoundException>()
            .WithMessage("User with ID 1 not found.");
    }


    [Fact]
    public void Update_WhenValidUser_ShouldCallDataContextUpdate()
    {
        // Arrange
        var service = CreateService();
        var user = new User { Id = 1, Forename = "Updated", Surname = "User", Email = "updated@example.com", DateOfBirth = DateTime.Now.AddYears(-35), IsActive = false };

        // Act
        service.Update(user);

        // Assert
        _dataContext.Verify(dc => dc.Update(It.Is<User>(u => u.Id == user.Id && u.Email == user.Email)), Times.Once);
    }

    [Fact]
    public void Delete_WhenUserExists_ShouldCallDataContextDelete()
    {
        // Arrange
        var service = CreateService();
        var user = new User { Id = 1, Forename = "To Delete", Surname = "User", Email = "todelete@example.com", DateOfBirth = DateTime.Now.AddYears(-40), IsActive = true };
        _dataContext.Setup(dc => dc.GetAll<User>()).Returns(new[] { user }.AsQueryable());

        // Act
        service.Delete(1);

        // Assert
        _dataContext.Verify(dc => dc.Delete(It.Is<User>(u => u.Id == user.Id)), Times.Once);
    }

    [Fact]
    public void Delete_WhenUserDoesNotExist_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var service = CreateService();
        _dataContext.Setup(dc => dc.GetAll<User>()).Returns(new User[] { }.AsQueryable());

        // Act & Assert
        service.Invoking(s => s.Delete(1))
            .Should().Throw<UserNotFoundException>()
            .WithMessage("User with ID 1 not found.");
    }

    [Fact]
    public void CreateLog_ShouldAddLogSuccessfully()
    {
        // Arrange
        var userService = new UserService(_dataContext.Object);
        var userId = 1;
        var action = "TestAction";
        var details = "Test details";

        // Act
        userService.CreateLog(userId, action, details);

        // Assert
        _dataContext.Verify(d => d.CreateLog(It.Is<Log>(l =>
            l.UserId == userId &&
            l.Action == action &&
            l.Details == details)), Times.Once);
    }

    [Fact]
    public void GetLogsForUser_ShouldReturnUserLogs()
    {
        // Arrange
        var userService = new UserService(_dataContext.Object);
        var userId = 1;
        var logs = new List<Log>
        {
            new Log { UserId = userId, Action = "Action1", Details = "Details1" },
            new Log { UserId = userId, Action = "Action2", Details = "Details2" }
        };
        _dataContext.Setup(d => d.GetLogsForUser(userId)).Returns(logs.AsQueryable());

        // Act
        var result = userService.GetLogsForUser(userId);

        // Assert
        result.Should().BeEquivalentTo(logs);
    }

    [Fact]
    public void GetAllLogs_ShouldReturnAllLogs()
    {
        // Arrange
        var userService = new UserService(_dataContext.Object);
        var logs = new List<Log>
        {
            new Log { UserId = 1, Action = "Action1", Details = "Details1" },
            new Log { UserId = 2, Action = "Action2", Details = "Details2" }
        };
        _dataContext.Setup(d => d.GetAllLogs()).Returns(logs.AsQueryable());

        // Act
        var result = userService.GetAllLogs();

        // Assert
        result.Should().BeEquivalentTo(logs);
    }

    [Fact]
    public void GetLogsForUser_NonExistentUser_ShouldReturnEmptyList()
    {
        // Arrange
        var userService = new UserService(_dataContext.Object);
        var nonExistentUserId = 999;
        _dataContext.Setup(d => d.GetLogsForUser(nonExistentUserId)).Returns(new List<Log>().AsQueryable());

        // Act
        var result = userService.GetLogsForUser(nonExistentUserId);

        // Assert
        result.Should().BeEmpty();
    }



    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
