using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using ProductServer;
using ProductServer.Constants;
using ProductServer.Infrastructure.Context;
using ProductServer.Infrastructure.Models;
using ProductServer.Infrastructure.Validators;
using SharedLibrary.Infrastructure.Requests;

var builder = WebApplication.CreateBuilder(args);

switch (builder.Configuration["DatabaseProvider"])
{
    case "MySql":
        builder.Services.AddDbContext<ProductContext, ProductMySqlContext>();
        break;

    case "AzureSql":
        builder.Services.AddDbContext<ProductContext, ProductAzureSqlContext>();
        break;
}

if (builder.Environment.IsProduction())
{
    builder.Services.AddOpenIddict()
        .AddValidation(options =>
        {
            options.SetIssuer("https://gdsctarumt-openid.azurewebsites.net");
            options.AddAudiences("product_server");

            options.UseIntrospection()
                .SetClientId("product_server")
                .SetClientSecret("product_server_secret");

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
            options.AddAudiences("product_server");

            options.UseIntrospection()
                .SetClientId("product_server")
                .SetClientSecret("product_server_secret");

            options.UseSystemNetHttp();

            options.UseAspNetCore();
        });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(ProductServerCorsDefaults.PolicyName, policy =>
    {
        policy.WithOrigins(ProductServerCorsDefaults.CorsOriginHttps, ProductServerCorsDefaults.CorsOriginHttp, "https://icy-flower-09eb00c00.2.azurestaticapps.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddHostedService<ProductWorker>();
builder.Services.AddScoped<IValidator<ProductRequest>, ProductRequestValidator>();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

app.UseCors(ProductServerCorsDefaults.PolicyName);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var publicImageStoragePath = Path.Combine(builder.Environment.ContentRootPath, "Storage", "Public", "Image");

if (!Directory.Exists(publicImageStoragePath))
    Directory.CreateDirectory(publicImageStoragePath);

// GET: Get image file
app.MapGet("/productServer/static/image/{imageFileName}/", (string imageFileName) =>
    Results.File(
        File.OpenRead(Path.Combine(publicImageStoragePath, imageFileName)),
        $"image/{imageFileName.Split(".")[1]}"
    ));

// POST: Upload image file
app.MapPost("/productServer/static/image/upload", async (IFormFile imageFile) =>
{
    if (imageFile.Length <= 0)
        return Results.BadRequest("Image file is required");

    if (!imageFile.ContentType.Contains("image"))
        return Results.BadRequest("File type must be image file type");

    var imageFileNameParts = imageFile.FileName.Split(".");
    var imageFileName = imageFileNameParts[0] + "_" + Guid.NewGuid() + "." + imageFileNameParts[1];

    if (!await SaveImageFileAsync(imageFile, imageFileName))
        return Results.BadRequest("Upload image failed");

    return Results.Ok(new
    {
        imageName = imageFileName
    });
}).RequireAuthorization();

// GET: Get All Product
app.MapGet("/productServer/product/", async (ProductContext context) =>
    context.Products == null
        ? Results.BadRequest()
        : Results.Ok(await context.Products.ToListAsync()))
    .RequireAuthorization();

// GET: Product with Id
app.MapGet("/productServer/product/{id:int}/", async (int id, ProductContext context) =>
{
    if (context.Products == null)
        return Results.BadRequest();

    var product = await context.Products.FindAsync(id);

    return product == null ? Results.BadRequest() : Results.Ok(product);
}).RequireAuthorization();

// POST: Create Product
app.MapPost("/productServer/product/",
    async (ProductRequest productRequest, ProductContext context, IValidator<ProductRequest> validator) =>
    {
        var result = await validator.ValidateAsync(productRequest);

        if (!result.IsValid)
            return Results.BadRequest(result.Errors.First().ErrorMessage);

        var newProduct = (Product)productRequest;

        try
        {
            await context.Products.AddAsync(newProduct);
            await context.SaveChangesAsync();
        }
        catch
        {
            return Results.BadRequest();
        }

        return Results.Ok(newProduct);
    }).RequireAuthorization();

// PUT: Update Product
app.MapPut("/productServer/product/{id:int}/",
    async (int id, ProductRequest productRequest, ProductContext context, IValidator<ProductRequest> validator) =>
    {
        productRequest.Id = id;
        var result = await validator.ValidateAsync(productRequest);

        if (!result.IsValid)
            return Results.BadRequest(result.Errors.First().ErrorMessage);

        var product = await context.Products.FindAsync(id);

        if (product == null)
            return Results.BadRequest("Product not found");

        try
        {
            product.Name = productRequest.Name;
            product.Description = productRequest.Description;
            product.OriginalPrice = productRequest.OriginalPrice;
            product.DiscountPercentage = productRequest.DiscountPercentage;

            context.Products.Update(product);
            await context.SaveChangesAsync();
        }
        catch
        {
            return Results.BadRequest();
        }

        return Results.Ok(product);
    }).RequireAuthorization();

// DELETE: Delete Product
app.MapDelete("/productServer/product/{id:int}/", async (int id, ProductContext context) =>
{
    var product = await context.Products.FindAsync(id);

    if (product == null)
        return Results.BadRequest("Product not found");

    context.Products.Remove(product);

    try
    {
        await context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.BadRequest();
    }

    return Results.Ok();
}).RequireAuthorization();

async Task<bool> SaveImageFileAsync(IFormFile imageFile, string filename)
{
    try
    {
        // TODO: Prevent saving same file

        await using var stream = File.Create(Path.Combine(publicImageStoragePath, filename));
        await imageFile.CopyToAsync(stream);

        return true;
    }
    catch
    {
        return false;
    }
}

await app.RunAsync();