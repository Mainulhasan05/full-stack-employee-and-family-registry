using Microsoft.AspNetCore.Identity;

namespace family_registry_backend.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
