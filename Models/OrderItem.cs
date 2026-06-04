namespace PizzaApp.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    
    // NEW: Fields to store pizza customization choices in SQLite
    public string Size { get; set; } = "Medium";
    public string Crust { get; set; } = "Thin";
}