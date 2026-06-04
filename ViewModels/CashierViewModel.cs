using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PizzaApp.Data;
using PizzaApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PizzaApp.ViewModels;

public partial class CashierViewModel : ViewModelBase
{
    private readonly Action _onLogout;

    // Lists holding our available database products
    public List<Product> PizzaMenu { get; }
    public List<Product> DrinkMenu { get; }

    // The live basket/shopping cart containing items the cashier clicks on
    public ObservableCollection<OrderItemViewModel> Cart { get; } = new();

    [ObservableProperty]
    private bool _isDelivery = false;

    [ObservableProperty]
    private string _deliveryAddress = string.Empty;

    [ObservableProperty]
    private decimal _cartTotal = 0.00m;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CashierViewModel(Action onLogout)
    {
        _onLogout = onLogout;

        // Pull our seeded pizzas and drinks out of the SQLite database
        using (var context = new AppDbContext())
        {
            PizzaMenu = context.Products.Where(p => p.Category == "Pizza").ToList();
            DrinkMenu = context.Products.Where(p => p.Category == "Drink").ToList();
        }
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        StatusMessage = string.Empty;
        var existingItem = Cart.FirstOrDefault(item => item.Product.Id == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            Cart.Add(new OrderItemViewModel(product));
        }

        RecalculateTotal();
    }

    [RelayCommand]
    private void RemoveFromCart(OrderItemViewModel item)
    {
        Cart.Remove(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        CartTotal = Cart.Sum(item => item.Product.Price * item.Quantity);
    }

    [RelayCommand]
    private void PlaceOrder()
    {
        if (!Cart.Any())
        {
            StatusMessage = "Cannot place an empty order!";
            return;
        }

        if (IsDelivery && string.IsNullOrWhiteSpace(DeliveryAddress))
        {
            StatusMessage = "Please enter a delivery address!";
            return;
        }

        using (var context = new AppDbContext())
        {
            var newOrder = new Order
            {
                Type = IsDelivery ? OrderType.Delivery : OrderType.Counter,
                Status = OrderStatus.Pending,
                DeliveryAddress = IsDelivery ? DeliveryAddress : null,
                TotalAmount = CartTotal,
                IsPaid = true // Paid immediately at counter
            };

            context.Orders.Add(newOrder);
            context.SaveChanges(); // Generates the Order ID

            // Save individual line items linked to this order
            foreach (var cartItem in Cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = newOrder.Id,
                    ProductId = cartItem.Product.Id,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Product.Price
                };
                context.OrderItems.Add(orderItem);
            }

            context.SaveChanges();
        }

        // Reset the screen for the next customer
        Cart.Clear();
        DeliveryAddress = string.Empty;
        IsDelivery = false;
        CartTotal = 0.00m;
        StatusMessage = "🎉 Order placed successfully and sent to the kitchen!";
    }

    [RelayCommand]
    private void Logout() => _onLogout.Invoke();
}

// Small helper wrapper to make cart row quantities reactive inside the UI list
public partial class OrderItemViewModel : ObservableObject
{
    public Product Product { get; }
    
    [ObservableProperty]
    private int _quantity = 1;

    public OrderItemViewModel(Product product) => Product = product;
}