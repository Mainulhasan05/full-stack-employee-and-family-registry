using System.ComponentModel.DataAnnotations;

namespace family_registry_backend.Models;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(17)]
    public string NID { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    public decimal BasicSalary { get; set; }

    // Navigation properties
    public Spouse? Spouse { get; set; }
    public ICollection<Child> Children { get; set; } = new List<Child>();
}
