using System.ComponentModel.DataAnnotations;

namespace family_registry_backend.Models;

public class Child
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    // Foreign key
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
