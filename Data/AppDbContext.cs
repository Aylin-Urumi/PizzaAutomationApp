using Microsoft.EntityFrameworkCore;
using PizzaApp.Models;

namespace PizzaApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=pizza.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed the 4 specific roles you requested
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "manager", PinCode = "1111", Role = "Manager" },
            new User { Id = 2, Username = "cashier1", PinCode = "2222", Role = "Cashier" },
            new User { Id = 3, Username = "chef1", PinCode = "3333", Role = "Chef" },
            new User { Id = 4, Username = "driver1", PinCode = "4444", Role = "Driver" }
        );
    }
}