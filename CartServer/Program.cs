using System.Text.Json;
using CartServer.Constants;
using CartServer.Infrastructure.Context;
using CartServer.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using SharedLibrary.Infrastructure.DataTransferObjects;
using SharedLibrary.Infrastructure.Requests;

var builder = WebApplication.CreateBuilder(args);

switch (builder.Configuration["DatabaseProvider"])
{
    case "MySql":
        builder.Services.AddDbContext<CartContext, CartMySqlContext>();
        break;

    case "AzureSql":
        builder.Services.AddDbContext<CartContext, CartAzureSqlContext>();
        break;
}

if (builder.Environment.IsProduction())
{    
    builder.Services.AddOpenIddict()
        .AddValidation(options =>
        {
            options.SetIssuer("https://gdsctarumt-openid.azurewebsites.net");
            options.AddAudiences("cart_server");

            options.UseIntrospection()
                .SetClientId("cart_server")
                .SetClientSecret("cart_server_secret");

            options.UseSystemNetHttp();

            options.UseAspNetCore();
        });

    builder.Services.AddLogging(options =>
    {
        options.AddAzureWebAppDiagnostics();
    });
}
else
{
    builder.Services.AddOpenIddict()
        .AddValidation(options =>
        {
            options.SetIssuer("https://localhost:4000");
            options.AddAudiences("cart_server");

            options.UseIntrospection()
                .SetClientId("cart_server")
                .SetClientSecret("cart_server_secret");

            options.UseSystemNetHttp();

            options.UseAspNetCore();
        });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CartServerCorsDefaults.PolicyName, policy =>
    {
        policy.WithOrigins(CartServerCorsDefaults.CorsOriginHttps, CartServerCorsDefaults.CorsOriginHttp)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(CartServerCorsDefaults.PolicyName);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// GET: Cart with Id
app.MapGet("/cartServer/cart/{id:int}/",
    async (int id, CartContext context) =>
        context.Carts == null
            ? Results.BadRequest()
            : Results.Ok((CartDto)await context.Carts.SingleOrDefaultAsync(m => m.Id == id))
).RequireAuthorization();

// POST: Create Cart
app.MapPost("/cartServer/cart/", async (CartRequest cartRequest, CartContext context) =>
{
    try
    {
        await context.Carts.AddAsync((Cart)cartRequest);
        await context.SaveChangesAsync();
    }
    catch
    {
        return Results.BadRequest();
    }

    return Results.Ok();
}).RequireAuthorization();

// PUT: Update Cart
app.MapPut("/cartServer/cart/{id:int}/", async (int id, CartRequest cartRequest, CartContext context) =>
{
    var cart = await context.Carts.FindAsync(id);

    if (cart == null)
        return Results.BadRequest("Cart not found");

    try
    {
        cart.ProductIds = JsonSerializer.Serialize(cartRequest.ProductIds);

        context.Carts.Update(cart);
        await context.SaveChangesAsync();
    }
    catch
    {
        return Results.BadRequest();
    }

    return Results.Ok();
}).RequireAuthorization();

// DELETE: Delete Cart
app.MapDelete("/cartServer/cart/{id:int}", async (int id, CartContext context) =>
{
    var cart = await context.Carts.FindAsync(id);

    if (cart == null)
        return Results.BadRequest("Cart not found");

    try
    {
        context.Carts.Remove(cart);
        await context.SaveChangesAsync();
    }
    catch
    {
        return Results.BadRequest();
    }

    return Results.Ok();
}).RequireAuthorization();

await app.RunAsync();