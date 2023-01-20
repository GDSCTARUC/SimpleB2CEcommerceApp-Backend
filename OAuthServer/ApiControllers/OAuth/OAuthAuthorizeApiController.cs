using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.Infrastructure.Context;
using OAuthServer.Infrastructure.Models;
using System.Text.Json;
using System.Web;

namespace OAuthServer.ApiControllers.OAuth
{
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class OAuthAuthorizeApiController : ControllerBase
    {
        private readonly OAuthContext _context;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public OAuthAuthorizeApiController(
            OAuthContext context,
            IDataProtectionProvider dataProtectionProvider
        )
        {
            _context = context;
            _dataProtectionProvider = dataProtectionProvider;
        }

        [HttpGet]
        [Route("/oauth/authorize")]
        public IActionResult Authorize()
        {
            HttpContext.Request.Query.TryGetValue("state", out var state);
            HttpContext.Request.Query.TryGetValue("client_id", out var clientId);
            HttpContext.Request.Query.TryGetValue("code_challenge", out var codeChallenge);
            HttpContext.Request.Query.TryGetValue("code_challenge_method", out var codeChallengeMethod);
            HttpContext.Request.Query.TryGetValue("redirect_uri", out var redirectUri);
            HttpContext.Request.Query.TryGetValue("scope", out var scope);
            var iss = HttpUtility.UrlEncode("https://localhost:4000");

            if (!HttpContext.Request.Query.TryGetValue("response_type", out var responseType))
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    state,
                    iss
                });
            }

            var protector = _dataProtectionProvider.CreateProtector("oauth");
            var authCode = new AuthCode
            {
                ClientId = clientId,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = codeChallengeMethod,
                RedirectUri = redirectUri,
                Expiry = DateTime.Now.AddMinutes(10),
            };

            var codeString = protector.Protect(JsonSerializer.Serialize(authCode));
            return Redirect($"{redirectUri}" +
                $"?code={codeString}" +
                $"&state={state}" +
                $"&iss={iss}");
        }
    }
}
