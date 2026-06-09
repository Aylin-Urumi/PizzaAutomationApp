using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

    // 🌟 THE ACCOUNTABILITY LINKS (Required by Cashier and Chef ViewModels) 🌟
    
    public int? CreatedByCashierId { get; set; }
    [ForeignKey("CreatedByCashierId")]
    public User? CreatedByCashier { get; set; }

    public int? AssignedChefId { get; set; }
    [ForeignKey("AssignedChefId")]
    public User? AssignedChef { get; set; }

    public int? AssignedDriverId { get; set; }
    [ForeignKey("AssignedDriverId")]
    public User? AssignedDriver { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}