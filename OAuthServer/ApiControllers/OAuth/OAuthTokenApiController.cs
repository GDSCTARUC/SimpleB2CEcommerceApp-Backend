using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OAuthServer.Infrastructure.Models;
using SharedLibrary.Utils;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OAuthServer.ApiControllers.OAuth
{
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class OAuthTokenApiController : ControllerBase
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly DevKeys _devKeys;

        public OAuthTokenApiController(
            IDataProtectionProvider dataProtectionProvider,
            DevKeys devKeys
        )
        {
            _dataProtectionProvider = dataProtectionProvider;
            _devKeys = devKeys;
        }

        [HttpPost]
        [Route("/oauth/token")]
        public async Task<IActionResult> Token()
        {
            var bodyBytes = await HttpContext.Request.BodyReader.ReadAsync();
            var bodyContent = Encoding.UTF8.GetString(bodyBytes.Buffer);

            string grantType = "", code = "", redirectUri = "", codeVerifier = "";
            foreach (var part in bodyContent.Split("&"))
            {
                var subParts = part.Split("=");
                var key = subParts[0];
                var value = subParts[1];

                if (key == "grant_type")
                {
                    grantType = value;
                }
                else if (key == "code")
                {
                    code = value;
                }
                else if (key == "redirect_uri")
                {
                    redirectUri = value;
                }
                else if (key == "code_verifier")
                {
                    codeVerifier = value;
                }
            }

            var protector = _dataProtectionProvider.CreateProtector("oauth");
            var codeString = protector.Unprotect(code);
            var authCode = JsonSerializer.Deserialize<AuthCode>(codeString);

            if (!ValidateCodeVerifier(authCode, codeVerifier))
            {
                return BadRequest();
            }

            var handler = new JsonWebTokenHandler();
            return Ok(new
            {
                access_token = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Claims = new Dictionary<string, object>
                    {
                        [JwtRegisteredClaimNames.Sub] = Guid.NewGuid().ToString(),
                    },
                    Expires = DateTime.Now.AddMinutes(10),
                    TokenType = "Bearer",
                    SigningCredentials = new SigningCredentials(
                        _devKeys.RsaSecurityKey, SecurityAlgorithms.RsaSha256),
                }),
                token_type = "Bearer"
            });
        }

        private static bool ValidateCodeVerifier(AuthCode authCode, string codeVerifier)
        {
            var codeChallenge = Base64UrlEncoder.Encode(
                SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)));
            return authCode.CodeChallenge == codeChallenge;
        }
    }
}
