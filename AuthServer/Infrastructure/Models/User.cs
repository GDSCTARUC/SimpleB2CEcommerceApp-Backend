using SharedLibrary.Infrastructure.DataTransferObjects;
using SharedLibrary.Infrastructure.Models;
using SharedLibrary.Infrastructure.Requests;

namespace AuthServer.Infrastructure.Models
{
    public class User : ModelBase
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHashed { get; set; }

        public static implicit operator UserDto(User user)
            => new()
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            };

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
