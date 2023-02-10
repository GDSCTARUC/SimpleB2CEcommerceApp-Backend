using FluentValidation;
using ProductServer.Infrastructure.Context;
using SharedLibrary.Infrastructure.Requests;

namespace ProductServer.Infrastructure.Validators;

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    private readonly ProductContext _context;

    public ProductRequestValidator(ProductContext context)
    {
        _context = context;

        RuleFor(m => m.Name)
            .NotNull().WithMessage("Product name is required")
            .Must(UniqueName).WithMessage("Product name is not available");
        RuleFor(m => m.OriginalPrice)
            .NotNull().WithMessage("Product original price is required");
    }

    private bool UniqueName(ProductRequest productRequest, string name)
    {
        var dbProduct = _context.Products.SingleOrDefault(m => m.Name == name);

        if (dbProduct == null)
            return true;

        if (productRequest.Id == dbProduct.Id && dbProduct.Name == name)
            return true;

        return dbProduct.Name != name;
    }
}