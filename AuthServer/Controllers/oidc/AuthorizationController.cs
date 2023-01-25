using AuthServer.Attributes;
using AuthServer.Extensions;
using AuthServer.Infrastructure.Context;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SharedLibrary.Infrastructure.DataTransferObjects;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Controllers.oidc
{
    public class AuthorizationController : Controller
    {
        private readonly AuthContext _context;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationController(
            AuthContext context,
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager
        )
        {
            _context = context;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
        }

        [HttpPost("~/connect/token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                return await HandleExchangeCodeGrantType();
            }

            if (request.IsClientCredentialsGrantType())
            {
                // The client credentials are automatically validated by OpenIddict,
                // if client_id or client_secret are invalid, this action won't be invoked.

                return await HandleExchangeClientCredentialsGrantType(request);
            }

            throw new InvalidOperationException("The specific grant type is not supported");
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved");

            // If prompt=login was specified by the client application,
            // immediately return the user agent to the login page.
            if (request.HasPrompt(Prompts.Login))
            {
                var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));
                var parameters = Request.HasFormContentType
                    ? Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList()
                    : Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

                parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            // Retrieve the user principal store in the authentication cookie.
            // If a max_age parameter was provided, ensure that the cookie is not too old.
            // If the user principal can't be extracted or the cookie is too old,
            // redirect the user agent to the login page.
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result == null ||
                !result.Succeeded ||
                (request.MaxAge != null &&
                result.Properties?.IssuedUtc != null &&
                DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
            {
                if (request.HasPrompt(Prompts.None))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }));
                }

                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            if (!int.TryParse(result.Principal.GetClaim(ClaimTypes.NameIdentifier), out var userId))
            {
                throw new InvalidOperationException("UserId is invalid");
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException("User is invalid");
            }

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId
                ?? throw new InvalidOperationException("Detials concerning the calling client application cannot be found"));

            var authorizations = await _authorizationManager.FindAsync(
                subject: userId.ToString(),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                // If the consent is external (e.g. when authorizations are granted by a sysadmin),
                // immediately return an error if no authorizations can be found in the database.
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }));

                // If the consent is implicit or if an authorizations was found,
                // return an authorizations response without displaying the consent form.
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                    // Convert to an OpenIddict Claim
                    var claims = new List<Claim>
                    {
                        new Claim(Claims.Subject, userId.ToString()),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    claimsIdentity.SetDestinations(GetDestinations);

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    claimsPrincipal.SetScopes(request.GetScopes());
                    claimsPrincipal.SetResources(await _scopeManager.ListResourcesAsync(claimsPrincipal.GetScopes()).ToListAsync());

                    var authorization = authorizations.LastOrDefault();
                    authorization ??= await _authorizationManager.CreateAsync(
                            principal: claimsPrincipal,
                            subject: userId.ToString(),
                            client: await _applicationManager.GetIdAsync(application),
                            type: AuthorizationTypes.Permanent,
                            scopes: claimsPrincipal.GetScopes());

                    claimsPrincipal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                    return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // At this point, no authorization was found in the database and an error must be returned
                // if the client application specified prompt=none in the authorization request.
                case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
                case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "Interactive user consent is required."
                        }));

                // In every other case, render the consent form.
                default:
                    return View(new ConsentFormDto
                    {
                        ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
                        Scope = request.Scope
                    });
            }
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthorizeAccept()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (!int.TryParse(User.GetClaim(ClaimTypes.NameIdentifier), out var userId))
            {
                throw new InvalidOperationException("UserId is invalid");
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException("User is invalid");
            }

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId
                ?? throw new InvalidOperationException("Detials concerning the calling client application cannot be found"));

            var authorizations = await _authorizationManager.FindAsync(
                subject: userId.ToString(),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            if (!authorizations.Any() && await _applicationManager.HasClientTypeAsync(application, ConsentTypes.External))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }));
            }

            // Convert to an OpenIddict Claim
            var claims = new List<Claim>
                    {
                        new Claim(Claims.Subject, userId.ToString()),
                    };

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            claimsIdentity.SetDestinations(GetDestinations);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            claimsPrincipal.SetScopes(request.GetScopes());
            claimsPrincipal.SetResources(await _scopeManager.ListResourcesAsync(claimsPrincipal.GetScopes()).ToListAsync());

            var authorization = authorizations.LastOrDefault();
            authorization ??= await _authorizationManager.CreateAsync(
                    principal: claimsPrincipal,
                    subject: userId.ToString(),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: claimsPrincipal.GetScopes());

            claimsPrincipal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public IActionResult AuthorizeDeny() =>
            Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        [HttpGet("~/connect/logout")]
        public IActionResult Logout() => View();

        [ActionName(nameof(Logout))]
        [HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
        }

        private async Task<IActionResult> HandleExchangeCodeGrantType()
        {
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private async Task<IActionResult> HandleExchangeClientCredentialsGrantType(OpenIddictRequest request)
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                throw new InvalidOperationException("The application details cannot be found in the database");
            }

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.AddClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
            identity.AddClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));
            identity.SetDestinations(GetDestinations);

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static IEnumerable<string> GetDestinations(Claim claim) =>
            claim.Type switch
            {
                Claims.Name or Claims.Subject => new[] { Destinations.AccessToken, Destinations.IdentityToken },
                _ => new[] { Destinations.AccessToken }
            };
    }
}
