using AuthServer.Infrastructure.Models;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Web;

namespace AuthServer.Endpoints.OAuth;

public static class AuthorizationEndpoint
{
    public static IResult PostHandler(
        HttpRequest httpRequest,
        IDataProtectionProvider dataProtectionProvider
    )
    {
        var iss = HttpUtility.UrlEncode("https://localhost:6010");
        _ = httpRequest.Query.TryGetValue("state", out var state);
        _ = httpRequest.Query.TryGetValue("client_id", out var clientId);
        _ = httpRequest.Query.TryGetValue("code_challenge", out var codeChallenge);
        _ = httpRequest.Query.TryGetValue("code_challenge_method", out var codeChallengeMethod);
        _ = httpRequest.Query.TryGetValue("redirect_uri", out var redirectUri);
        _ = httpRequest.Query.TryGetValue("scope", out _);

        var protector = dataProtectionProvider.CreateProtector("oauth");
        var authCode = new AuthCode
        {
            ClientId = clientId,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            RedirectUri = redirectUri,
            Expiry = DateTime.Now.AddMinutes(10)
        };

        var codeString = protector.Protect(JsonSerializer.Serialize(authCode));
        return Results.Redirect($"{redirectUri}" +
                                $"?code={codeString}" +
                                $"&state={state}" +
                                $"&iss={iss}");
    }
}