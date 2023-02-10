using System.Text.Json;
using SharedLibrary.Infrastructure.DataTransferObjects;
using SharedLibrary.Infrastructure.Models;
using SharedLibrary.Infrastructure.Requests;

namespace CartServer.Infrastructure.Models;

public class Cart : ModelBase
{
    public int UserId { get; set; }
    public string ProductIds { get; set; }

    public static implicit operator CartDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            ProductIds = JsonSerializer.Deserialize<List<int>>(cart.ProductIds),
            UpdatedAt = cart.UpdatedAt,
            CreatedAt = cart.CreatedAt
        };
    }

    public static explicit operator Cart(CartRequest cartRequest)
    {
        return new Cart
        {
            UserId = cartRequest.UserId,
            ProductIds = JsonSerializer.Serialize(cartRequest.ProductIds)
        };
    }
}