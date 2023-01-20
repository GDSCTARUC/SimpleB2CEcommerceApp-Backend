using SharedLibrary.Infrastructure.Models;

namespace AuthServer.Infrastructure.Models;

public class User : ModelBase
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}