using family_registry_backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace family_registry_backend.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = { "Admin", "Viewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed default admin user
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new AppUser
            {
                UserName = "admin",
                Email = "admin@employee.local",
                FullName = "System Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed default viewer user
        if (await userManager.FindByNameAsync("viewer") == null)
        {
            var viewer = new AppUser
            {
                UserName = "viewer",
                Email = "viewer@employee.local",
                FullName = "Default Viewer",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(viewer, "Viewer@123");
            await userManager.AddToRoleAsync(viewer, "Viewer");
        }

        // Seed employees if none exist
        if (!await context.Employees.AnyAsync())
        {
            var employees = GetSeedEmployees();
            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();
        }
    }

    private static List<Employee> GetSeedEmployees()
    {
        return new List<Employee>
        {
            new()
            {
                Name = "Md. Hasan Mahmud",
                NID = "1990123456789",
                Phone = "+8801712345678",
                Department = "Engineering",
                BasicSalary = 55000,
                Spouse = new Spouse { Name = "Fatema Akter", NID = "1991234567890" },
                Children = new List<Child>
                {
                    new() { Name = "Arif Hasan", DateOfBirth = new DateTime(2018, 3, 15) },
                    new() { Name = "Ayesha Hasan", DateOfBirth = new DateTime(2020, 7, 22) }
                }
            },
            new()
            {
                Name = "Moushumi Rahman",
                NID = "8823456712",
                Phone = "01812345678",
                Department = "Human Resources",
                BasicSalary = 48000,
                Spouse = new Spouse { Name = "Kamal Uddin", NID = "8834567823" },
                Children = new List<Child>
                {
                    new() { Name = "Nusrat Rahman", DateOfBirth = new DateTime(2015, 11, 5) }
                }
            },
            new()
            {
                Name = "Tanvir Ahmed",
                NID = "19951234567890123",
                Phone = "+8801912345678",
                Department = "Finance",
                BasicSalary = 62000,
                Spouse = new Spouse { Name = "Shabnam Nahar", NID = "19961234567890124" },
                Children = new List<Child>
                {
                    new() { Name = "Rafid Ahmed", DateOfBirth = new DateTime(2019, 1, 10) },
                    new() { Name = "Rania Ahmed", DateOfBirth = new DateTime(2021, 6, 18) },
                    new() { Name = "Rayan Ahmed", DateOfBirth = new DateTime(2023, 9, 2) }
                }
            },
            new()
            {
                Name = "Nasima Begum",
                NID = "8845678934",
                Phone = "01612345678",
                Department = "Administration",
                BasicSalary = 42000
            },
            new()
            {
                Name = "Rafiqul Islam",
                NID = "19881234567890125",
                Phone = "+8801512345678",
                Department = "Engineering",
                BasicSalary = 70000,
                Spouse = new Spouse { Name = "Rehana Akter", NID = "19891234567890126" },
                Children = new List<Child>
                {
                    new() { Name = "Saiful Islam", DateOfBirth = new DateTime(2012, 4, 20) }
                }
            },
            new()
            {
                Name = "Sharmin Sultana",
                NID = "8856789045",
                Phone = "01312345678",
                Department = "Marketing",
                BasicSalary = 50000,
                Spouse = new Spouse { Name = "Jahidul Haque", NID = "8867890156" },
                Children = new List<Child>
                {
                    new() { Name = "Labib Haque", DateOfBirth = new DateTime(2017, 8, 30) },
                    new() { Name = "Lamisa Haque", DateOfBirth = new DateTime(2020, 2, 14) }
                }
            },
            new()
            {
                Name = "Kamal Hossain",
                NID = "19921234567890127",
                Phone = "+8801412345678",
                Department = "Finance",
                BasicSalary = 58000,
                Spouse = new Spouse { Name = "Tahmina Khatun", NID = "19931234567890128" }
            },
            new()
            {
                Name = "Nusrat Jahan",
                NID = "8878901267",
                Phone = "01712345679",
                Department = "Human Resources",
                BasicSalary = 45000
            },
            new()
            {
                Name = "Zahidul Haque",
                NID = "19851234567890129",
                Phone = "+8801612345679",
                Department = "Engineering",
                BasicSalary = 75000,
                Spouse = new Spouse { Name = "Sumaiya Akter", NID = "19861234567890130" },
                Children = new List<Child>
                {
                    new() { Name = "Zarin Haque", DateOfBirth = new DateTime(2016, 5, 25) },
                    new() { Name = "Zarif Haque", DateOfBirth = new DateTime(2019, 12, 1) }
                }
            },
            new()
            {
                Name = "Farhana Yasmin",
                NID = "8889012378",
                Phone = "01812345679",
                Department = "Administration",
                BasicSalary = 47000,
                Spouse = new Spouse { Name = "Monir Hossain", NID = "8890123489" },
                Children = new List<Child>
                {
                    new() { Name = "Farhan Hossain", DateOfBirth = new DateTime(2014, 10, 8) }
                }
            }
        };
    }
}
