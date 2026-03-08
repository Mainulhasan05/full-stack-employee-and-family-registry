namespace family_registry_backend.DTOs;

// --- Request DTOs ---

public class EmployeeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public SpouseDto? Spouse { get; set; }
    public List<ChildDto> Children { get; set; } = new();
}

public class EmployeeUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public SpouseDto? Spouse { get; set; }
    public List<ChildDto> Children { get; set; } = new();
}

// --- Response DTOs ---

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public SpouseResponseDto? Spouse { get; set; }
    public List<ChildResponseDto> Children { get; set; } = new();
}

public class SpouseDto
{
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
}

public class SpouseResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
}

public class ChildDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class ChildResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}
