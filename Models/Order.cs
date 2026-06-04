using System;
using System.Collections.Generic;

namespace PizzaApp.Models;

// Simplified down to the two distinct workflows of your restaurant
public enum OrderType { Counter, Delivery }
public enum OrderStatus { Pending, Cooking, Ready, OutForDelivery, Completed }

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Only filled out if Type == OrderType.Delivery
    public string? DeliveryAddress { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    // Since they pay upfront at the counter, this will almost always be true once placed!
    public bool IsPaid { get; set; } = true;

    public List<OrderItem> Items { get; set; } = new();
}