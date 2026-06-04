using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PizzaApp.Data;
using PizzaApp.Models;

namespace PizzaApp.ViewModels;

// FIX 1: Inherit from ViewModelBase instead of ObservableObject
public partial class CashierViewModel : ViewModelBase
{
    // FIX 2: Move this field above the properties and ensure it stays purely local
    private Action? _onLogoutAction;

    // Expose filtered sub-menus directly to the XAML view bindings
    public IEnumerable<Product> PizzaMenu => Products.Where(p => p.Category == "Pizza");
    public IEnumerable<Product> DrinkMenu => Products.Where(p => p.Category == "Drinks" || p.Category == "Drink");

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<OrderItemViewModel> _cart = new();

    [ObservableProperty]
    private decimal _cartTotal = 0.00m;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _deliveryAddress = string.Empty;

    [ObservableProperty]
    private bool _isDelivery;

    // Corrected constructor setting the local private action variable safely
    public CashierViewModel(Action onLogout)
    {
        _onLogoutAction = onLogout;
        LoadProducts();
    }

    [RelayCommand]
    private void Logout() => _onLogoutAction?.Invoke();

    private void LoadProducts()
    {
        using (var context = new AppDbContext())
        {
            var productList = context.Products.ToList();
            Products = new ObservableCollection<Product>(productList);
        }

        // Alert the Avalonia rendering engine that our sub-menus are populated and ready to draw
        OnPropertyChanged(nameof(PizzaMenu));
        OnPropertyChanged(nameof(DrinkMenu));
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        StatusMessage = string.Empty;

        // Always add a fresh line item for pizzas to allow unique size/crust customization combos
        if (product.Category == "Pizza")
        {
            Cart.Add(new OrderItemViewModel(product, RecalculateTotal));
        }
        else
        {
            // For drinks, we can group matching items together directly
            var existingItem = Cart.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                Cart.Add(new OrderItemViewModel(product, RecalculateTotal));
            }
        }

        RecalculateTotal();
    }

    [RelayCommand]
    private void RemoveFromCart(OrderItemViewModel item)
    {
        if (item == null) return;
        Cart.Remove(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        CartTotal = Cart.Sum(item => item.ComputedPrice * item.Quantity);
    }

    [RelayCommand]
    private void PlaceOrder()
    {
        if (!Cart.Any())
        {
            StatusMessage = "❌ Cannot place an empty order!";
            return;
        }

        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            StatusMessage = "❌ Please enter a Customer Name!";
            return;
        }

        if (IsDelivery)
        {
            if (string.IsNullOrWhiteSpace(DeliveryAddress))
            {
                StatusMessage = "❌ Please enter a delivery address!";
                return;
            }
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                StatusMessage = "❌ Please enter a phone number for delivery!";
                return;
            }
        }

        using (var context = new AppDbContext())
        {
            var newOrder = new Order
            {
                Type = IsDelivery ? OrderType.Delivery : OrderType.Counter,
                Status = OrderStatus.Pending,
                CustomerName = CustomerName.Trim(),
                PhoneNumber = IsDelivery ? PhoneNumber.Trim() : null,
                DeliveryAddress = IsDelivery ? DeliveryAddress.Trim() : null,
                TotalAmount = CartTotal,
                IsPaid = true
            };

            context.Orders.Add(newOrder);
            context.SaveChanges();

            foreach (var cartItem in Cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = newOrder.Id,
                    ProductId = cartItem.Product.Id,
                    Quantity = cartItem.Quantity,
                    // Saves the custom dynamic price calculation straight to the invoice log
                    PriceAtPurchase = cartItem.ComputedPrice,
                    Size = cartItem.Product.Category == "Pizza" ? cartItem.Size : "N/A",
                    Crust = cartItem.Product.Category == "Pizza" ? cartItem.Crust : "N/A"
                };
                context.OrderItems.Add(orderItem);
            }

            context.SaveChanges();
        }

        // Reset form inputs for the next customer interaction session
        Cart.Clear();
        CustomerName = string.Empty;
        PhoneNumber = string.Empty;
        DeliveryAddress = string.Empty;
        IsDelivery = false;
        CartTotal = 0.00m;
        StatusMessage = "🎉 Order placed successfully and sent to the kitchen!";
    }

    // Automatically wipes conditional delivery text values if the cashier unchecks the delivery option
    partial void OnIsDeliveryChanged(bool value)
    {
        if (!value)
        {
            PhoneNumber = string.Empty;
            DeliveryAddress = string.Empty;
        }
    }
}

// REACTIVE LINE ITEM DESIGN PATTERN WITH BUILT-IN ADAPTIVE UPSCALING
public partial class OrderItemViewModel : ObservableObject
{
    private readonly Action _onChanged;
    public Product Product { get; }

    [ObservableProperty] private int _quantity = 1;
    [ObservableProperty] private string _size = "Medium";
    [ObservableProperty] private string _crust = "Thin";

    // Item-level arrays for frontend dropdown elements
    public string[] Sizes => new[] { "Small", "Medium", "Large" };
    public string[] Crusts => new[] { "Thin", "Deep Dish", "Stuffed" };

    // Layout filtering utility flag
    public bool IsPizza => Product.Category == "Pizza";

    public OrderItemViewModel(Product product, Action onChanged)
    {
        Product = product;
        _onChanged = onChanged;
    }

    // Dynamic price logic modifier matrix
    public decimal ComputedPrice
    {
        get
        {
            decimal basePrice = Product.Price;
            if (Product.Category != "Pizza") return basePrice;

            // Size Modifier Logic
            if (Size == "Small") basePrice -= 2.00m;  // Price reductions for Small sizes
            if (Size == "Large") basePrice += 3.50m;  // Price adjustments for Large sizes

            // Crust Modifier Logic
            if (Crust == "Deep Dish") basePrice += 1.50m;
            if (Crust == "Stuffed") basePrice += 2.50m;

            return basePrice;
        }
    }

    // Automatically notifies the primary view model to re-sum calculations on change instances
    partial void OnQuantityChanged(int value) => _onChanged();
    partial void OnSizeChanged(string value) { OnPropertyChanged(nameof(ComputedPrice)); _onChanged(); }
    partial void OnCrustChanged(string value) { OnPropertyChanged(nameof(ComputedPrice)); _onChanged(); }
}