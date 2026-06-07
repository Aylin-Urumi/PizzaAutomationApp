namespace PizzaApp.Models;

public class ProductIngredient
{
    public int Id { get; set; }
    
    // Links back to your existing menu products
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // Links to the raw ingredient
    public int IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }

    // Amount consumed per single unit of this product
    public double QuantityNeeded { get; set; }
}