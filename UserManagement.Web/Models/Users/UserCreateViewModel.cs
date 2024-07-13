using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models.Users;

public class UserCreateViewModel
{
    [Required]
    [Display(Name = "First Name")]
    public string? Forename { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    public string? Surname { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
