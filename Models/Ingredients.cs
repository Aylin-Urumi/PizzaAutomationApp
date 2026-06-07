namespace PizzaApp.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double StockQuantity { get; set; }
    public string Unit { get; set; } = string.Empty; // e.g., "grams", "pieces"
}