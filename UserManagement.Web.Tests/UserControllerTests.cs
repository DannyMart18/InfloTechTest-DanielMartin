using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Exceptions;
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
        model.Items.Should().BeEquivalentTo(users, options => options
            .Excluding(u => u.Logs)); // Exclude Logs from comparison
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
        model.Items.Should().BeEquivalentTo(users, options => options
            .Excluding(u => u.Logs)); // Exclude Logs from comparison
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
        model.Items.Should().BeEquivalentTo(allUsers, options => options
            .Excluding(u => u.Logs)); // Exclude Logs from comparison
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

    [Fact]
    public void List_ShouldIncludeDateOfBirthInViewModel()
    {
        // Arrange
        var controller = CreateController();
        var expectedDate = new DateTime(1990, 1, 1);
        SetupUsers(dateOfBirth: expectedDate);

        // Act
        var result = controller.List() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        var model = result!.Model as UserListViewModel;
        model.Should().NotBeNull();
        model!.Items.Should().ContainSingle();
        model.Items.First().DateOfBirth.Should().Be(expectedDate);
    }

    [Fact]
    public void Create_GET_ReturnsViewWithEmptyModel()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<UserCreateViewModel>();
    }

    [Fact]
    public void Create_POST_ValidModel_RedirectsToListWithSuccessMessage()
    {
        // Arrange
        var controller = CreateController();
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        var model = new UserCreateViewModel
        {
            Forename = "New",
            Surname = "User",
            Email = "newuser@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };

        // Act
        var result = controller.Create(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UsersController.List));
        controller.TempData["SuccessMessage"].Should().Be("User created successfully.");
    }

    [Fact]
    public void Create_POST_InvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Invalid email");
        var model = new UserCreateViewModel();

        // Act
        var result = controller.Create(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
    }

    [Fact]
    public void Edit_GET_ExistingUser_ReturnsViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 1, Forename = "Existing", Surname = "User", Email = "existing@example.com", DateOfBirth = new DateTime(1990, 1, 1), IsActive = true };
        _userService.Setup(s => s.GetById(1)).Returns(user);

        // Act
        var result = controller.Edit(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UserCreateViewModel>().Subject;
        model.Should().BeEquivalentTo(user, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public void Edit_GET_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        _userService.Setup(s => s.GetById(1)).Throws(new UserNotFoundException("User not found"));

        // Act
        var result = controller.Edit(1);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Edit_POST_ValidModel_RedirectsToListWithSuccessMessage()
    {
        // Arrange
        var controller = CreateController();
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        var model = new UserCreateViewModel { Forename = "Updated", Surname = "User", Email = "updated@example.com", DateOfBirth = new DateTime(1990, 1, 1), IsActive = true };
        _userService.Setup(s => s.GetById(1)).Returns(new User { Id = 1 });

        // Act
        var result = controller.Edit(1, model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UsersController.List));
        controller.TempData["SuccessMessage"].Should().Be("User updated successfully.");
    }

    [Fact]
    public void View_ExistingUser_ReturnsViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 1, Forename = "Existing", Surname = "User", Email = "existing@example.com", DateOfBirth = new DateTime(1990, 1, 1), IsActive = true };
        _userService.Setup(s => s.GetById(1)).Returns(user);

        // Act
        var result = controller.View(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UserViewModel>().Subject;
        model.Should().BeEquivalentTo(user, options => options
            .Excluding(u => u.Logs)); // Exclude Logs from comparison
    }

    [Fact]
    public void View_NonExistingUser_ThrowsUserNotFoundException()
    {
        // Arrange
        var controller = CreateController();
        _userService.Setup(s => s.GetById(1)).Throws(new UserNotFoundException("User not found"));

        // Act & Assert
        controller.Invoking(c => c.View(1))
            .Should().Throw<UserNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public void Delete_GET_ExistingUser_ReturnsViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 1, Forename = "To Delete", Surname = "User", Email = "todelete@example.com", DateOfBirth = new DateTime(1990, 1, 1), IsActive = true };
        _userService.Setup(s => s.GetById(1)).Returns(user);

        // Act
        var result = controller.Delete(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeEquivalentTo(user);
    }

    [Fact]
    public void Delete_POST_ExistingUser_RedirectsToListWithSuccessMessage()
    {
        // Arrange
        var controller = CreateController();
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _userService.Setup(s => s.Delete(1)).Verifiable();

        // Act
        var result = controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UsersController.List));
        controller.TempData["SuccessMessage"].Should().Be("User deleted successfully.");
        _userService.Verify(s => s.Delete(1), Times.Once);
    }

    [Fact]
    public void Delete_POST_NonExistingUser_RedirectsToListWithErrorMessage()
    {
        // Arrange
        var controller = CreateController();
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _userService.Setup(s => s.Delete(1)).Throws(new UserNotFoundException("User not found"));

        // Act
        var result = controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(UsersController.List));
        controller.TempData["ErrorMessage"].Should().Be("An error occurred while deleting the user.");
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true, DateTime? dateOfBirth = null)
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
            new User { Forename = "Active", Surname = "User", Email = "active@example.com", IsActive = isActive, DateOfBirth = DateTime.Now.AddYears(-30)},
            new User { Forename = "Another", Surname = "User", Email = "another@example.com", IsActive = isActive, DateOfBirth = DateTime.Now.AddYears(-30)}
        };

        _userService
            .Setup(s => s.FilterByActive(isActive))
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}
