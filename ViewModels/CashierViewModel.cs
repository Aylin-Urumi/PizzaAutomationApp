using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PizzaApp.Data;
using PizzaApp.Models;

namespace PizzaApp.ViewModels;

public partial class CashierViewModel : ViewModelBase
{
    private Action? _onLogoutAction;

    // 🌟 NEW: Track exactly which cashier profile is placing orders
    [ObservableProperty]
    private User _currentCashier;

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

    [ObservableProperty]
    private ObservableCollection<User> _drivers = new();

    [ObservableProperty]
    private User? _selectedDriver;

    // 🌟 NEW: Properties to populate the chef assignment dropdown selections
    [ObservableProperty]
    private ObservableCollection<User> _availableChefs = new();

    [ObservableProperty]
    private User? _selectedChef;

    [ObservableProperty]
    private string _feedbackCustomerName = string.Empty;

    [ObservableProperty]
    private string _feedbackDetails = string.Empty;

    public IEnumerable<Product> PizzaMenu => Products.Where(p => p.Category == "Pizza");
    public IEnumerable<Product> DrinkMenu => Products.Where(p => p.Category == "Drinks" || p.Category == "Drink");

    // 🌟 UPDATED: Constructor accepts the authenticated Cashier user profile
    public CashierViewModel(User authenticatedCashier, Action onLogout)
    {
        _currentCashier = authenticatedCashier;
        _onLogoutAction = onLogout;
        LoadInitialData();
    }

    [RelayCommand]
    private void Logout() => _onLogoutAction?.Invoke();

    private void LoadInitialData()
    {
        using (var context = new AppDbContext())
        {
            // Load standard menu items
            var productList = context.Products.ToList();
            Products = new ObservableCollection<Product>(productList);

            // Fetch all user accounts registered under the "Driver" or "Delivery" roles
            var driverList = context.Users
                .Where(u => u.Role == "Driver" || u.Role == "Delivery")
                .ToList();
            Drivers = new ObservableCollection<User>(driverList);

            // 🌟 NEW: Dynamically pull down all staff users assigned to the Kitchen/Chef role
            var chefList = context.Users
                .Where(u => u.Role == "Chef")
                .ToList();
            AvailableChefs = new ObservableCollection<User>(chefList);
        }

        OnPropertyChanged(nameof(PizzaMenu));
        OnPropertyChanged(nameof(DrinkMenu));
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        StatusMessage = string.Empty;
        if (product.Category == "Pizza")
        {
            Cart.Add(new OrderItemViewModel(product, RecalculateTotal));
        }
        else
        {
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

        // 🌟 NEW: Safety validation check ensuring a kitchen staff member has been chosen
        if (SelectedChef == null)
        {
            StatusMessage = "❌ Please assign a kitchen chef to cook this order!";
            return;
        }

        if (IsDelivery)
        {
            if (string.IsNullOrWhiteSpace(DeliveryAddress) || string.IsNullOrWhiteSpace(PhoneNumber))
            {
                StatusMessage = "❌ Please enter Address and Phone Number for Delivery!";
                return;
            }
            if (SelectedDriver == null)
            {
                StatusMessage = "❌ Please assign a delivery guy to this order!";
                return;
            }
        }

        try
        {
            using (var context = new AppDbContext())
            {
                // =================================================================
                // PHASE 1: STOCK GUARD PRE-CHECK (Do we have enough inventory?)
                // =================================================================
                foreach (var cartItem in Cart)
                {
                    var recipes = context.ProductIngredients
                        .Where(pi => pi.ProductId == cartItem.Product.Id)
                        .ToList();

                    foreach (var recipe in recipes)
                    {
                        var ingredient = context.Ingredients.FirstOrDefault(i => i.Id == recipe.IngredientId);
                        if (ingredient != null)
                        {
                            double totalNeeded = recipe.QuantityNeeded * cartItem.Quantity;

                            if (ingredient.StockQuantity < totalNeeded)
                            {
                                StatusMessage = $"❌ Order Failed! Out of '{ingredient.Name}'. (Needed: {totalNeeded} {ingredient.Unit}, Available: {ingredient.StockQuantity} {ingredient.Unit})";
                                return; 
                            }
                        }
                    }
                }

                // =================================================================
                // PHASE 2: CONSTRUCT ORDER RECORD (With Full Audit Log Accountability)
                // =================================================================
                var newOrder = new Order
                {
                    Type = IsDelivery ? OrderType.Delivery : OrderType.Counter,
                    Status = OrderStatus.Pending,
                    CustomerName = CustomerName.Trim(),
                    PhoneNumber = IsDelivery ? PhoneNumber.Trim() : null,
                    DeliveryAddress = IsDelivery ? DeliveryAddress.Trim() : null,
                    TotalAmount = CartTotal,
                    IsPaid = true,
                    
                    // 🌟 FULL WORKFORCE COMPLIANCE ASSIGNMENT 🌟
                    CreatedByCashierId = CurrentCashier.Id,
                    AssignedChefId = SelectedChef.Id,
                    AssignedDriverId = IsDelivery ? SelectedDriver?.Id : null
                };

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
                    newOrder.Items.Add(orderItem);
                }

                context.Orders.Add(newOrder);

                // =================================================================
                // PHASE 3: DEDUCT STOCK FROM INVENTORY
                // =================================================================
                foreach (var cartItem in Cart)
                {
                    var recipes = context.ProductIngredients
                        .Where(pi => pi.ProductId == cartItem.Product.Id)
                        .ToList();

                    foreach (var recipe in recipes)
                    {
                        var ingredient = context.Ingredients.FirstOrDefault(i => i.Id == recipe.IngredientId);
                        if (ingredient != null)
                        {
                            double totalDeduction = recipe.QuantityNeeded * cartItem.Quantity;
                            ingredient.StockQuantity -= totalDeduction;

                            if (ingredient.StockQuantity < 0)
                            {
                                ingredient.StockQuantity = 0;
                            }
                        }
                    }
                }

                context.SaveChanges();
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new OrderChangedMessage());
            }

            // Clear the basket items
            Cart.Clear();

            // Clear fields
            PhoneNumber = string.Empty;
            DeliveryAddress = string.Empty;
            IsDelivery = false;
            SelectedDriver = null;
            SelectedChef = null; // 🌟 Reset chef picker state

            StatusMessage = $"🎉 Order placed!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Database Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SubmitFeedback()
    {
        if (string.IsNullOrWhiteSpace(FeedbackCustomerName) || string.IsNullOrWhiteSpace(FeedbackDetails))
        {
            StatusMessage = "❌ Please fill in both Customer Name and Feedback Details fields!";
            return;
        }

        try
        {
            using (var context = new AppDbContext())
            {
                var feedbackEntry = new CustomerFeedback
                {
                    CustomerName = FeedbackCustomerName.Trim(),
                    Details = FeedbackDetails.Trim(),
                    DateSubmitted = DateTime.Now
                };

                context.CustomerFeedbacks.Add(feedbackEntry);
                context.SaveChanges();
            }

            FeedbackCustomerName = string.Empty;
            FeedbackDetails = string.Empty;
            StatusMessage = "💖 Feedback captured securely for Manager analysis!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Feedback Error: {ex.Message}";
        }
    }

    partial void OnIsDeliveryChanged(bool value)
    {
        if (!value)
        {
            PhoneNumber = string.Empty;
            DeliveryAddress = string.Empty;
            SelectedDriver = null;
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