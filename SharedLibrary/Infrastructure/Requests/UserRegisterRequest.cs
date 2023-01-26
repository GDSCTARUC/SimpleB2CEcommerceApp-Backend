using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Infrastructure.Requests;

public class UserRegisterRequest
{
    [Required]
    [DisplayName("Username")]
    public string Username { get; set; }
    [Required(ErrorMessage = "The First Name field is required. *")]
    [DisplayName("First Name")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "The Last Name field is required. *")]
    [DisplayName("Last Name")]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    [DisplayName("Email")]
    public string Email { get; set; }
    [Required]
    [DisplayName("Password")]
    public string Password { get; set; }
    [Required(ErrorMessage = "The Password Retype field is required. *")]
    [DisplayName("Password Retype")]
    public string PasswordRetype { get; set; }
}