using Microsoft.EntityFrameworkCore;
using PizzaApp.Models;

namespace PizzaApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // NEW: This constructor guarantees SQLite creates tables matching our models
    public AppDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=pizza.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Seed our staff user accounts
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "manager", PinCode = "1111", Role = "Manager" },
            new User { Id = 2, Username = "cashier1", PinCode = "2222", Role = "Cashier" },
            new User { Id = 3, Username = "chef1", PinCode = "3333", Role = "Chef" },
            new User { Id = 4, Username = "driver1", PinCode = "4444", Role = "Driver" }
        );

        // 2. Seed our initial 3 Pizzas and 3 Drinks
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Margherita Pizza", Price = 10.99m, Category = "Pizza" },
            new Product { Id = 2, Name = "Pepperoni Pizza", Price = 12.99m, Category = "Pizza" },
            new Product { Id = 3, Name = "BBQ Chicken Pizza", Price = 13.99m, Category = "Pizza" },
            new Product { Id = 4, Name = "Coca Cola", Price = 1.99m, Category = "Drink" },
            new Product { Id = 5, Name = "Sprite", Price = 1.99m, Category = "Drink" },
            new Product { Id = 6, Name = "Bottled Water", Price = 0.99m, Category = "Drink" }
        );
    }
}