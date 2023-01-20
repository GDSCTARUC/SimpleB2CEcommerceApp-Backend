using SharedLibrary.Infrastructure.Models;
using SharedLibrary.Infrastructure.Request;

namespace OAuthServer.Infrastructure.Models
{
    public class User : ModelBase
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHashed { get; set; }

        public static explicit operator User(UserRegisterRequest userRegisterRequest)
            => new()
            {
                Username = userRegisterRequest.Username,
                FirstName = userRegisterRequest.FirstName,
                LastName = userRegisterRequest.LastName,
                Email = userRegisterRequest.Email,
                PasswordHashed = null
            };
    }
}
