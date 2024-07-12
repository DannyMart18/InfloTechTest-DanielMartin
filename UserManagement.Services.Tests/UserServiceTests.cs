using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

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


    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
