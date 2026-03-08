using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using family_registry_backend.Models;

namespace family_registry_backend.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Spouse> Spouses => Set<Spouse>();
    public DbSet<Child> Children => Set<Child>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Employee configuration
        builder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.NID).IsUnique();
            e.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
        });

        // Spouse — one-to-one with Employee
        builder.Entity<Spouse>(s =>
        {
            s.HasIndex(x => x.NID).IsUnique();
            s.HasOne(x => x.Employee)
             .WithOne(x => x.Spouse)
             .HasForeignKey<Spouse>(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Child — one-to-many with Employee
        builder.Entity<Child>(c =>
        {
            c.HasOne(x => x.Employee)
             .WithMany(x => x.Children)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
