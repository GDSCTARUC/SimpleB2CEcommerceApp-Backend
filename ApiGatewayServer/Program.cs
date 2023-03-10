using ApiGatewayServer.Constants;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();
builder.Environment.ContentRootPath = Directory.GetCurrentDirectory();

builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddJsonFile("ocelot.json")
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

if (builder.Environment.IsProduction())
{
    builder.Services.AddLogging(options =>
    {
        options.AddAzureWebAppDiagnostics();
    });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(ApiGatewayServerCorsDefaults.PolicyName, policy =>
    {
        policy.WithOrigins(ApiGatewayServerCorsDefaults.CorsOriginHttps, ApiGatewayServerCorsDefaults.CorsOriginHttp, "https://icy-flower-09eb00c00.2.azurestaticapps.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors(ApiGatewayServerCorsDefaults.PolicyName);
app.UseHttpsRedirection();
app.UseOcelot().Wait();

await app.RunAsync();