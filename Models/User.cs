namespace PizzaApp.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    
    // For a simple terminal, a 4-digit PIN works great
    public string PinCode { get; set; } = string.Empty; 
    
    // Roles: "Admin", "Cashier", "Kitchen"
    public string Role { get; set; } = "Cashier"; 
}