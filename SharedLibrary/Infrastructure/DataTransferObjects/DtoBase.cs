namespace SharedLibrary.Infrastructure.DataTransferObjects
{
    public class DtoBase
    {
        public int Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
