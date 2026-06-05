using System;
using System.Collections.Generic;

namespace PizzaApp.Models;

public enum OrderType { Counter, Delivery }
public enum OrderStatus { Pending, Cooking, Ready, OutForDelivery, Completed }

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? DeliveryAddress { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; } = true;

    public int? AssignedDriverId { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}