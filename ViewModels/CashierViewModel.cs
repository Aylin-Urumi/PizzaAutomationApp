using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PizzaApp.Data;
using PizzaApp.Models;

namespace PizzaApp.ViewModels;

public partial class CashierViewModel : ViewModelBase
{
    private Action? _onLogoutAction;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<OrderItemViewModel> _cart = new();

    [ObservableProperty]
    private decimal _cartTotal = 0.00m;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Customer Detail Fields
    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _deliveryAddress = string.Empty;

    [ObservableProperty]
    private bool _isDelivery;

    // Filtered sub-menus mapped directly to the UI tabs
    public IEnumerable<Product> PizzaMenu => Products.Where(p => p.Category == "Pizza");
    public IEnumerable<Product> DrinkMenu => Products.Where(p => p.Category == "Drinks" || p.Category == "Drink");

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

        // Notify rendering pipeline that sub-categories are populated
        OnPropertyChanged(nameof(PizzaMenu));
        OnPropertyChanged(nameof(DrinkMenu));
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        StatusMessage = string.Empty;

        // Pizzas always bypass grouping to allow unique topping/crust combinations
        if (product.Category == "Pizza")
        {
            Cart.Add(new OrderItemViewModel(product, RecalculateTotal));
        }
        else
        {
            // Beverages group seamlessly on matching IDs
            var existingItem = Cart.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem != null)
                existingItem.Quantity++;
            else
                Cart.Add(new OrderItemViewModel(product, RecalculateTotal));
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

        if (IsDelivery && (string.IsNullOrWhiteSpace(DeliveryAddress) || string.IsNullOrWhiteSpace(PhoneNumber)))
        {
            StatusMessage = "❌ Please enter Address and Phone Number for Delivery!";
            return;
        }

        try
        {
            using (var context = new AppDbContext())
            {
                // 1. Create the master order record
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

                // 2. Map basket directly into the Order's item collection
                foreach (var cartItem in Cart)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.Product.Id,
                        Quantity = cartItem.Quantity,
                        PriceAtPurchase = cartItem.ComputedPrice,
                        Size = cartItem.Product.Category == "Pizza" ? cartItem.Size : "N/A",
                        Crust = cartItem.Product.Category == "Pizza" ? cartItem.Crust : "N/A",
                        Toppings = cartItem.Product.Category == "Pizza"
                            ? string.Join(", ", cartItem.ToppingOptions.Where(t => t.IsSelected).Select(t => t.Name))
                            : ""
                    };

                    // EF Core links relationship details automatically via navigation lists
                    newOrder.Items.Add(orderItem);
                }

                // 3. Save the entire tree transaction at once
                context.Orders.Add(newOrder);
                context.SaveChanges();
            }

            // Reset UI inputs upon execution success
            Cart.Clear();
            CustomerName = string.Empty;
            PhoneNumber = string.Empty;
            DeliveryAddress = string.Empty;
            IsDelivery = false;
            CartTotal = 0.00m;
            StatusMessage = "🎉 Order placed successfully and sent to the kitchen!";
        }
        catch (Exception ex)
        {
            // Catches any schema bugs and renders them clearly on screen instead of crashing
            StatusMessage = $"❌ Database Error: {ex.Message}";
        }
    }

    partial void OnIsDeliveryChanged(bool value)
    {
        if (!value)
        {
            PhoneNumber = string.Empty;
            DeliveryAddress = string.Empty;
        }
    }
}

public partial class OrderItemViewModel : ObservableObject
{
    private readonly Action _onChanged;
    public Product Product { get; }

    [ObservableProperty] private int _quantity = 1;
    [ObservableProperty] private string _size = "Medium";
    [ObservableProperty] private string _crust = "Thin";

    public string[] Sizes => new[] { "Small", "Medium", "Large" };
    public string[] Crusts => new[] { "Thin", "Deep Dish", "Stuffed" };

    public ObservableCollection<ToppingItem> ToppingOptions { get; } = new();
    public bool IsPizza => Product.Category == "Pizza";

    public OrderItemViewModel(Product product, Action onChanged)
    {
        Product = product;
        _onChanged = onChanged;

        if (IsPizza)
        {
            ToppingOptions.Add(new ToppingItem("Extra Cheese", 1.50m, ToppingChanged));
            ToppingOptions.Add(new ToppingItem("Pepperoni", 1.25m, ToppingChanged));
            ToppingOptions.Add(new ToppingItem("Mushrooms", 1.00m, ToppingChanged));
            ToppingOptions.Add(new ToppingItem("Onions", 0.75m, ToppingChanged));
        }
    }

    private void ToppingChanged()
    {
        OnPropertyChanged(nameof(ComputedPrice));
        _onChanged();
    }

    public decimal ComputedPrice
    {
        get
        {
            decimal basePrice = Product.Price;
            if (Product.Category != "Pizza") return basePrice;

            if (Size == "Small") basePrice -= 2.00m;
            if (Size == "Large") basePrice += 3.50m;

            if (Crust == "Deep Dish") basePrice += 1.50m;
            if (Crust == "Stuffed") basePrice += 2.50m;

            basePrice += ToppingOptions.Where(t => t.IsSelected).Sum(t => t.Price);
            return basePrice;
        }
    }

    partial void OnQuantityChanged(int value) => _onChanged();
    partial void OnSizeChanged(string value) { OnPropertyChanged(nameof(ComputedPrice)); _onChanged(); }
    partial void OnCrustChanged(string value) { OnPropertyChanged(nameof(ComputedPrice)); _onChanged(); }
}

public partial class ToppingItem : ObservableObject
{
    private readonly Action _onChanged;
    public string Name { get; }
    public decimal Price { get; }

    [ObservableProperty]
    private bool _isSelected;

    public ToppingItem(string name, decimal price, Action onChanged)
    {
        Name = name;
        Price = price;
        _onChanged = onChanged;
    }

    partial void OnIsSelectedChanged(bool value) => _onChanged();
}