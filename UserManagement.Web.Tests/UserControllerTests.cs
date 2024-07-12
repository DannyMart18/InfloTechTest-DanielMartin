using System;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenNoFilterProvided_ModelMustContainAllUsers()
    {
        // Arrange
        var controller = CreateController();
        var users = SetupUsers();

        // Act
        var result = controller.List();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UserListViewModel>().Subject;
        model.Items.Should().BeEquivalentTo(users);
        model.ActiveFilter.Should().Be("all");
    }

    [Theory]
    [InlineData("active", true)]
    [InlineData("inactive", false)]
    public void List_WhenFilterProvided_ModelMustContainFilteredUsers(string filter, bool isActive)
    {
        // Arrange
        var controller = CreateController();
        var users = SetupFilteredUsers(isActive);

        // Act
        var result = controller.List(filter);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UserListViewModel>().Subject;
        model.Items.Should().BeEquivalentTo(users);
        model.ActiveFilter.Should().Be(filter);
    }

    [Fact]
    public void List_WhenInvalidFilterProvided_ShouldReturnAllUsersAndSetFilterToAll()
    {
        // Arrange
        var controller = CreateController();
        var allUsers = SetupUsers();

        // Act
        var result = controller.List("invalidFilter");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UserListViewModel>().Subject;
        model.Items.Should().BeEquivalentTo(allUsers);
        model.ActiveFilter.Should().Be("all");
    }

    [Fact]
    public void List_WhenServiceThrowsException_ShouldReturnErrorView()
    {
        // Arrange
        _userService
            .Setup(s => s.GetAll())
            .Throws(new Exception("Service error"));

        var controller = CreateController();

        // Act
        var result = controller.List();

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.ViewName.Should().Be("Error");
    }



    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        return users;
    }

    private User[] SetupFilteredUsers(bool isActive)
    {
        var users = new[]
        {
            new User { Forename = "Active", Surname = "User", Email = "active@example.com", IsActive = isActive },
            new User { Forename = "Another", Surname = "User", Email = "another@example.com", IsActive = isActive }
        };

        _userService
            .Setup(s => s.FilterByActive(isActive))
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}
