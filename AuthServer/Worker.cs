using AuthServer.Infrastructure.Context;
using OpenIddict.Abstractions;

namespace AuthServer
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serverProvider;

        public Worker(IServiceProvider serverProvider)
        {
            _serverProvider = serverProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serverProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AuthContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            if (await applicationManager.FindByClientIdAsync("postman", cancellationToken) == null)
            {
                await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "postman",
                    ClientSecret = "postman-secret",
                    DisplayName = "Postman Client",
                    RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Address,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Phone,
                        OpenIddictConstants.Permissions.Scopes.Roles,

                        OpenIddictConstants.Permissions.Prefixes.Scope + "cart_api",
                        OpenIddictConstants.Permissions.Prefixes.Scope + "product_api",

                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                }, cancellationToken);
            }

            if (await applicationManager.FindByClientIdAsync("frontend", cancellationToken) == null)
            {
                await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "frontend",
                    DisplayName = "Frontend Client",
                    RedirectUris = { new Uri("https://localhost:3000") },
                    PostLogoutRedirectUris = { new Uri("https://localhost:3000") },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Logout,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Address,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Phone,
                        OpenIddictConstants.Permissions.Scopes.Roles,

                        OpenIddictConstants.Permissions.Prefixes.Scope + "cart_api",
                        OpenIddictConstants.Permissions.Prefixes.Scope + "product_api",

                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                }, cancellationToken);
            }

            if (await applicationManager.FindByClientIdAsync("cart_server", cancellationToken) == null)
            {
                await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "cart_server",
                    ClientSecret = "cart_server_secret",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                    }
                }, cancellationToken);
            }

            if (await applicationManager.FindByClientIdAsync("product_server", cancellationToken) == null)
            {
                await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "product_server",
                    ClientSecret = "product_server_secret",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                    }
                }, cancellationToken);
            }

            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
            if (await scopeManager.FindByNameAsync("cart_api", cancellationToken) == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "cart_api",
                    Resources = 
                    { 
                        "cart_server" 
                    }
                }, cancellationToken);
            }

            if (await scopeManager.FindByNameAsync("product_api", cancellationToken) == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "product_api",
                    Resources =
                    {
                        "product_server"
                    }
                }, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
