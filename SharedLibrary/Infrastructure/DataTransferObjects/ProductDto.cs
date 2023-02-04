namespace SharedLibrary.Infrastructure.DataTransferObjects;

public class ProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string ImageFileName { get; set; }
}