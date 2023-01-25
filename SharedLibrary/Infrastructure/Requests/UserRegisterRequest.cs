using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Infrastructure.Requests;

public class UserRegisterRequest
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string PasswordRetype { get; set; }
}