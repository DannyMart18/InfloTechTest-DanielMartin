using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public ActionResult List(string filter = "all")
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
}
