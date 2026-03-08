using System.ComponentModel.DataAnnotations;

namespace family_registry_backend.Models;

public class Spouse
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(17)]
    public string NID { get; set; } = string.Empty;

    // Foreign key
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
