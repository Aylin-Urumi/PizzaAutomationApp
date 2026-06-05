using System;

namespace PizzaApp.Models;

public class CustomerFeedback
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime DateSubmitted { get; set; } = DateTime.Now;
}