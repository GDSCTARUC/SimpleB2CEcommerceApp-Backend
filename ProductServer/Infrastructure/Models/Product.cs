using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedLibrary.Infrastructure.DataTransferObjects;
using SharedLibrary.Infrastructure.Models;
using SharedLibrary.Infrastructure.Requests;

namespace ProductServer.Infrastructure.Models;

public class Product : ModelBase
{
    [Required] public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(9, 2)")]
    public decimal OriginalPrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(9, 2)")]
    public decimal DiscountPercentage { get; set; }

    public string ImageFileName { get; set; }

    public static implicit operator ProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            OriginalPrice = product.OriginalPrice,
            DiscountPercentage = product.DiscountPercentage,
            ImageFileName = product.ImageFileName,
            UpdatedAt = product.UpdatedAt,
            CreatedAt = product.CreatedAt
        };
    }

    public static explicit operator Product(ProductRequest productDto)
    {
        return new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            OriginalPrice = productDto.OriginalPrice,
            DiscountPercentage = productDto.DiscountPercentage,
            ImageFileName = productDto.ImageFileName
        };
    }
}