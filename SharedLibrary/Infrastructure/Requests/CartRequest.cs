namespace SharedLibrary.Infrastructure.Requests;

public class CartRequest
{
    public int UserId { get; set; }
    public List<int> ProductIds { get; set; }
}