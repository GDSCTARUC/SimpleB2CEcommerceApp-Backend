namespace SharedLibrary.Infrastructure.Models;

public class ModelBase
{
    public ModelBase()
    {
        UpdatedAt = DateTime.Now;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}