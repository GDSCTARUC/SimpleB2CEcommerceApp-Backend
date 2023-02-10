namespace SharedLibrary.Infrastructure.DataTransferObjects;

public class CartDto : DtoBase
{
    public int UserId { get; set; }
    public List<int> ProductIds { get; set; }
}