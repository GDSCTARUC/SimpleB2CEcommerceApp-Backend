using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Infrastructure.Models;

/// <summary>
///     Model base for every model in this project.
///     DatabaseGeneratedOption.Computed: Value generated on add or update.
///     DatabaseGeneratedOption.Identity: Value generated on add.
///     DatabaseGeneratedOption.None: Value never generate.
/// </summary>
public class ModelBase
{
    [Required] [Key] public int Id { get; set; }

    [Required] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}