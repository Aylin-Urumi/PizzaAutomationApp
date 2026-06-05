using System.Linq;
using PizzaApp.Models;

namespace PizzaApp.Data;

public static class DatabaseSeeder
{
    public static void Seed()
    {
        using (var context = new AppDbContext())
        {
            // Ensure the database exists
            context.Database.EnsureCreated();

            // 1. STOCK THE EMPLOYEES (If none exist)
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { Username = "manager1", Role = "Manager", PinCode = "1111" },
                    new User { Username = "cashier1", Role = "Cashier", PinCode = "2222" },
                    new User { Username = "chef1", Role = "Chef", PinCode = "3333" },
                    new User { Username = "driver1", Role = "Driver", PinCode = "4444" }
                );
            }

            // 2. STOCK THE MENU ITEMS (If none exist)
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    // Pizzas
                    new Product { Name = "Margherita Pizza", Price = 10.99M },
                    new Product { Name = "Pepperoni Supreme", Price = 12.99M },
                    new Product { Name = "BBQ Chicken Pizza", Price = 14.99M },
                    new Product { Name = "Vegetarian Delight", Price = 11.99M },
                    
                    // Beverages
                    new Product { Name = "Coca Cola", Price = 1.99M },
                    new Product { Name = "Sprite", Price = 1.99M },
                    new Product { Name = "Iced Tea", Price = 2.25M },
                    new Product { Name = "Bottled Water", Price = 1.25M }
                );
            }

            // Save changes to the SQLite database file
            context.SaveChanges();
        }
    }
}