using System;
using System.Linq;
using FluentAssertions;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public void GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com"
        };
        context.Create(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        context.Delete(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public void Update_WhenEntityModified_MustUpdateEntity()
    {
        // Arrange
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        var originalId = entity.Id;
        entity.Forename = "Updated";
        entity.DateOfBirth = new DateTime(1995, 5, 5);

        // Act
        context.Update(entity);
        var updatedEntity = context.GetAll<User>().FirstOrDefault(u => u.Id == originalId);

        // Assert
        updatedEntity.Should().NotBeNull();
        updatedEntity.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetAll_WhenFilteredByActive_MustOnlyReturnActiveUsers()
    {
        // Arrange
        var context = CreateContext();
        context.Create(new User { Forename = "Active", Surname = "User", Email = "active@example.com", IsActive = true });
        context.Create(new User { Forename = "Inactive", Surname = "User", Email = "inactive@example.com", IsActive = false });

        // Act
        var result = context.GetAll<User>().Where(u => u.IsActive);

        // Assert
        result.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public void GetAll_WhenFilteredByInactive_MustOnlyReturnInactiveUsers()
    {
        // Arrange
        var context = CreateContext();
        context.Create(new User { Forename = "Active", Surname = "User", Email = "active@example.com", IsActive = true });
        context.Create(new User { Forename = "Inactive", Surname = "User", Email = "inactive@example.com", IsActive = false });

        // Act
        var result = context.GetAll<User>().Where(u => !u.IsActive);

        // Assert
        result.Should().OnlyContain(u => !u.IsActive);
    }


    private DataContext CreateContext() => new();
}
