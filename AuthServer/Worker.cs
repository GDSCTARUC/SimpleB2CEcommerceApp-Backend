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

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            if (await manager.FindByClientIdAsync("postman", cancellationToken) == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
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
                        
                        OpenIddictConstants.Permissions.Prefixes.Scope + "api",

                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync("frontend", cancellationToken) == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
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

                        OpenIddictConstants.Permissions.Prefixes.Scope + "api",

                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync("cart-server", cancellationToken) == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "cart-server",
                    ClientSecret = "cart-server-secret",
                    DisplayName = "Cart Server",
                    RedirectUris = { new Uri("https://localhost:5010/auth/") },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Introspection,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Address,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Phone,
                        OpenIddictConstants.Permissions.Scopes.Roles,

                        OpenIddictConstants.Permissions.Prefixes.Scope + "api",

                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync("product-server", cancellationToken) == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "product-server",
                    ClientSecret = "product-server-secret",
                    DisplayName = "Product Server",
                    RedirectUris = { new Uri("https://localhost:5020/auth/") },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Introspection,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Address,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Phone,
                        OpenIddictConstants.Permissions.Scopes.Roles,

                        OpenIddictConstants.Permissions.Prefixes.Scope + "api",

                        OpenIddictConstants.Permissions.ResponseTypes.Code
                    }
                }, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
