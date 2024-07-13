using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Exceptions;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("")]
    public ViewResult List(string filter = "all")
    {
        try
        {
            IEnumerable<User> users;
            switch (filter.ToLower())
            {
                case "active":
                    users = _userService.FilterByActive(true);
                    break;
                case "inactive":
                    users = _userService.FilterByActive(false);
                    break;
                default:
                    users = _userService.GetAll();
                    filter = "all";
                    break;
            }

            var items = users.Select(p => new UserListItemViewModel
            {
                Id = p.Id,
                Forename = p.Forename,
                Surname = p.Surname,
                Email = p.Email,
                IsActive = p.IsActive,
                DateOfBirth = p.DateOfBirth,
            });

            var model = new UserListViewModel
            {
                Items = items.ToList(),
                ActiveFilter = filter
            };

            return View(model);
        }
        catch (Exception)
        {
            return View("Error");
        }
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new UserCreateViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var user = new User
                {
                    Forename = model.Forename!,
                    Surname = model.Surname!,
                    Email = model.Email!,
                    DateOfBirth = model.DateOfBirth,
                    IsActive = model.IsActive
                };

                _userService.Create(user);
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(List));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the user.");
            }
        }
        return View(model);
    }

    [HttpGet("edit/{id}")]
    public IActionResult Edit(long id)
    {
        try
        {
            var user = _userService.GetById(id);
            var model = new UserCreateViewModel
            {
                Forename = user.Forename,
                Surname = user.Surname,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive
            };
            return View(model);
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, UserCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var user = _userService.GetById(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Forename = model.Forename!;
                user.Surname = model.Surname!;
                user.Email = model.Email!;
                user.DateOfBirth = model.DateOfBirth;
                user.IsActive = model.IsActive;

                _userService.Update(user);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(List));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the user.");
            }
        }
        return View(model);
    }

    [HttpGet("view/{id}")]
    public IActionResult View(long id)
    {
        var user = _userService.GetById(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new UserViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpGet("delete/{id}")]
    public IActionResult Delete(long id)
    {
        var user = _userService.GetById(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(long id)
    {
        try
        {
            _userService.Delete(id);
            TempData["SuccessMessage"] = "User deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the user.";
        }
        return RedirectToAction(nameof(List));
    }
}
