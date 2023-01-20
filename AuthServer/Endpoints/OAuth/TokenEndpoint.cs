using AuthServer.Infrastructure.Models;
using AuthServer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AuthServer.Endpoints.OAuth;

public static class TokenEndpoint
{
    public static async Task<IResult> PostHandler(
        HttpRequest httpRequest,
        DevKeys devKeys,
        IDataProtectionProvider dataProtectionProvider
    )
    {
        var bodyBytes = await httpRequest.BodyReader.ReadAsync();
        var bodyContent = Encoding.UTF8.GetString(bodyBytes.Buffer);
        string code = "", codeVerifier = "";
        foreach (var part in bodyContent.Split("&"))
        {
            var subParts = part.Split('=');
            var key = subParts[0];
            var value = subParts[1];

            switch (key)
            {
                case "grant_type":
                    string grantType = value;
                    break;
                case "code":
                    code = value;
                    break;
                case "redirect_uri":
                    string redirectUri = value;
                    break;
                case "code_verifier":
                    codeVerifier = value;
                    break;
            }
        }

        var protector = dataProtectionProvider.CreateProtector("oauth");
        var codeString = protector.Unprotect(code);
        var authCode = JsonSerializer.Deserialize<AuthCode>(codeString);
        if (ValidateCodeVerifier(authCode, codeVerifier)) return Results.BadRequest();

        var handler = new JsonWebTokenHandler();
        return Results.Ok(new
        {
            access_token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = Guid.NewGuid().ToString()
                },
                Expires = DateTime.Now.AddMinutes(10),
                TokenType = "Bearer",
                SigningCredentials = new SigningCredentials(
                    devKeys.RsaSecurityKey, SecurityAlgorithms.Sha256)
            }),
            token_type = "Bearer"
        });
    }

    private static bool ValidateCodeVerifier(AuthCode authCode, string codeVerifier)
    {
        var codeChallenge = Base64UrlEncoder.Encode(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(codeVerifier)
            )
        );
        return authCode.CodeChallenge == codeChallenge;
    }
}