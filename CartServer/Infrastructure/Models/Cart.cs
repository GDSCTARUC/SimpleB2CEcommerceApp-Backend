using System.Text.Json;
using SharedLibrary.Infrastructure.DataTransferObjects;
using SharedLibrary.Infrastructure.Models;
using SharedLibrary.Infrastructure.Requests;

namespace CartServer.Infrastructure.Models;

public class Cart : ModelBase
{
    public string ProductIds { get; set; }

    public static implicit operator CartDto(Cart cart)
    {
        return new CartDto
        {
            ProductIds = JsonSerializer.Deserialize<List<int>>(cart.ProductIds)
        };
    }

    public static explicit operator Cart(CartRequest cartRequest)
    {
        return new Cart
        {
            ProductIds = JsonSerializer.Serialize(cartRequest.ProductIds)
        };
    }
}