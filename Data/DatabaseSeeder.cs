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
                    // Pizzas - EXACT UI MATCHES
                    new Product { Name = "Margherita Pizza", Price = 10.99M },
                    new Product { Name = "Pepperoni Pizza", Price = 12.99M }, // FIXED NAME
                    new Product { Name = "BBQ Chicken Pizza", Price = 14.99M },
                    
                    // Beverages - EXACT UI MATCHES
                    new Product { Name = "Coca Cola", Price = 1.99M },
                    new Product { Name = "Sprite", Price = 1.99M },
                    new Product { Name = "Bottled Water", Price = 1.25M }
                );
            }

            // Save changes early so our newly added products generate their primary keys
            context.SaveChanges();

            // 3. NEW: STOCK RAW INGREDIENTS WAREHOUSE (If none exist)
            if (!context.Ingredients.Any())
            {
                context.Ingredients.AddRange(
                    // Pizza Raw Ingredients
                    new Ingredient { Name = "Pizza Dough Base", StockQuantity = 50, Unit = "pcs" },
                    new Ingredient { Name = "Mozzarella Cheese", StockQuantity = 12000, Unit = "g" },
                    new Ingredient { Name = "Tomato Sauce", StockQuantity = 8000, Unit = "g" },
                    new Ingredient { Name = "Pepperoni Slices", StockQuantity = 1500, Unit = "pcs" },
                    new Ingredient { Name = "Grilled Chicken Strips", StockQuantity = 4000, Unit = "g" },
                    new Ingredient { Name = "Sweet BBQ Sauce", StockQuantity = 3000, Unit = "g" },
                    
                    // Beverage Physical Stock (Added so Manager can track them!)
                    new Ingredient { Name = "Coca Cola Can", StockQuantity = 100, Unit = "cans" },
                    new Ingredient { Name = "Sprite Can", StockQuantity = 100, Unit = "cans" },
                    new Ingredient { Name = "Bottled Water Bottle", StockQuantity = 100, Unit = "bottles" }
                );
            }

            // Save ingredients so they generate their primary keys as well
            context.SaveChanges();

            // 4. NEW: MAP PRODUCTS TO INGREDIENTS BRIDGE (Recipes)
            if (!context.ProductIngredients.Any())
            {
                // Pull database entries into memory to securely match IDs dynamically by string names
                var products = context.Products.ToList();
                var ingredients = context.Ingredients.ToList();

                // Find Pizzas
                var margherita = products.FirstOrDefault(p => p.Name == "Margherita Pizza");
                var pepperoni = products.FirstOrDefault(p => p.Name == "Pepperoni Pizza"); // FIXED!
                var bbqChicken = products.FirstOrDefault(p => p.Name == "BBQ Chicken Pizza");
                
                // Find Beverages
                var cokeProd = products.FirstOrDefault(p => p.Name == "Coca Cola");
                var spriteProd = products.FirstOrDefault(p => p.Name == "Sprite");
                var waterProd = products.FirstOrDefault(p => p.Name == "Bottled Water");

                // Find Pizza Ingredients
                var dough = ingredients.FirstOrDefault(i => i.Name == "Pizza Dough Base");
                var cheese = ingredients.FirstOrDefault(i => i.Name == "Mozzarella Cheese");
                var sauce = ingredients.FirstOrDefault(i => i.Name == "Tomato Sauce");
                var pepSlices = ingredients.FirstOrDefault(i => i.Name == "Pepperoni Slices");
                var chicken = ingredients.FirstOrDefault(i => i.Name == "Grilled Chicken Strips");
                var bbqSauce = ingredients.FirstOrDefault(i => i.Name == "Sweet BBQ Sauce");

                // Find Beverage Stock
                var cokeStock = ingredients.FirstOrDefault(i => i.Name == "Coca Cola Can");
                var spriteStock = ingredients.FirstOrDefault(i => i.Name == "Sprite Can");
                var waterStock = ingredients.FirstOrDefault(i => i.Name == "Bottled Water Bottle");

                // --- BUILD PIZZA RECIPES ---
                if (margherita != null && dough != null && cheese != null && sauce != null)
                {
                    context.ProductIngredients.AddRange(
                        new ProductIngredient { ProductId = margherita.Id, IngredientId = dough.Id, QuantityNeeded = 1 },
                        new ProductIngredient { ProductId = margherita.Id, IngredientId = cheese.Id, QuantityNeeded = 150 }, // 150g
                        new ProductIngredient { ProductId = margherita.Id, IngredientId = sauce.Id, QuantityNeeded = 100 }   // 100g
                    );
                }

                if (pepperoni != null && dough != null && cheese != null && sauce != null && pepSlices != null)
                {
                    context.ProductIngredients.AddRange(
                        new ProductIngredient { ProductId = pepperoni.Id, IngredientId = dough.Id, QuantityNeeded = 1 },
                        new ProductIngredient { ProductId = pepperoni.Id, IngredientId = cheese.Id, QuantityNeeded = 150 },
                        new ProductIngredient { ProductId = pepperoni.Id, IngredientId = sauce.Id, QuantityNeeded = 100 },
                        new ProductIngredient { ProductId = pepperoni.Id, IngredientId = pepSlices.Id, QuantityNeeded = 20 } // 20 slices
                    );
                }

                if (bbqChicken != null && dough != null && cheese != null && bbqSauce != null && chicken != null)
                {
                    context.ProductIngredients.AddRange(
                        new ProductIngredient { ProductId = bbqChicken.Id, IngredientId = dough.Id, QuantityNeeded = 1 },
                        new ProductIngredient { ProductId = bbqChicken.Id, IngredientId = cheese.Id, QuantityNeeded = 120 },
                        new ProductIngredient { ProductId = bbqChicken.Id, IngredientId = bbqSauce.Id, QuantityNeeded = 80 },
                        new ProductIngredient { ProductId = bbqChicken.Id, IngredientId = chicken.Id, QuantityNeeded = 100 }
                    );
                }

                // --- BUILD BEVERAGE LINKS (1 Menu Drink = 1 Stock Drink) ---
                if (cokeProd != null && cokeStock != null)
                {
                    context.ProductIngredients.Add(new ProductIngredient { ProductId = cokeProd.Id, IngredientId = cokeStock.Id, QuantityNeeded = 1 });
                }

                if (spriteProd != null && spriteStock != null)
                {
                    context.ProductIngredients.Add(new ProductIngredient { ProductId = spriteProd.Id, IngredientId = spriteStock.Id, QuantityNeeded = 1 });
                }

                if (waterProd != null && waterStock != null)
                {
                    context.ProductIngredients.Add(new ProductIngredient { ProductId = waterProd.Id, IngredientId = waterStock.Id, QuantityNeeded = 1 });
                }

                // Save final recipe structures to database
                context.SaveChanges();
            }
        }
    }
}