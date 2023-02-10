namespace SharedLibrary.Infrastructure.DataTransferObjects;

public class UserDto : DtoBase
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
}