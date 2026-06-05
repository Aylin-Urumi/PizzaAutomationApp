using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PizzaApp.Data;
using PizzaApp.Models;

namespace PizzaApp.ViewModels;

// FIXED: Added the IRecipient interface implementation to satisfy the compiler and handle real-time kitchen syncs
public partial class ChefViewModel : ViewModelBase, IRecipient<OrderChangedMessage>
{
    private readonly Action _onLogoutAction;

    [ObservableProperty]
    private ObservableCollection<Order> _cookingOrders = new();

    [ObservableProperty]
    private string _chefStatusMessage = string.Empty;

    public ChefViewModel(Action onLogout)
    {
        _onLogoutAction = onLogout;

        // Load initial cooking screen orders
        LoadChefOrders();

        // FIXED: Explicitly registers this kitchen instance to the messenger channel bus
        WeakReferenceMessenger.Default.Register<OrderChangedMessage>(this);
    }

    // REQUIRED METHOD: This automatically fires when an OrderChangedMessage is broadcasted anywhere in the app
    public void Receive(OrderChangedMessage message)
    {
        // Forces Avalonia's UI thread to cleanly re-fetch kitchen tickets safely
        Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadChefOrders());
    }

    [RelayCommand]
    private void Logout()
    {
        // Clean up messenger registrations on logout to prevent memory leaks
        WeakReferenceMessenger.Default.UnregisterAll(this);
        _onLogoutAction?.Invoke();
    }

    [RelayCommand]
    public void LoadChefOrders()
    {
        try
        {
            using (var context = new AppDbContext())
            {
                // Fetch all orders that are still being processed or cooked (exclude Completed/Ready)
                var ordersList = context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Cooking)
                    .OrderBy(o => o.OrderDate)
                    .ToList();

                CookingOrders = new ObservableCollection<Order>(ordersList);
            }
        }
        catch (Exception ex)
        {
            ChefStatusMessage = $"❌ Error loading tickets: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AdvanceStatus(Order order)
    {
        if (order == null) return;
        ChefStatusMessage = string.Empty;

        try
        {
            using (var context = new AppDbContext())
            {
                var dbOrder = context.Orders.FirstOrDefault(o => o.Id == order.Id);
                if (dbOrder != null)
                {
                    // Kitchen Workflow State Machine
                    if (dbOrder.Status == OrderStatus.Pending)
                    {
                        dbOrder.Status = OrderStatus.Cooking;
                        ChefStatusMessage = $"🍳 Order #{order.Id} is now cooking!";
                    }
                    else if (dbOrder.Status == OrderStatus.Cooking)
                    {
                        // FIX: Check if it requires a driver. 
                        // If it's delivery, mark it Ready so drivers can see it.
                        // If it's takeout (Counter), mark it Completed immediately!
                        if (dbOrder.Type == OrderType.Delivery)
                        {
                            dbOrder.Status = OrderStatus.Ready;
                            ChefStatusMessage = $"🍕 Delivery Order #{order.Id} is ready for dispatch!";
                        }
                        else
                        {
                            dbOrder.Status = OrderStatus.Completed;
                            ChefStatusMessage = $"✅ Takeout Order #{order.Id} has been picked up & finalized!";
                        }
                    }

                    context.SaveChanges();
                }
            }

            // Broadcast the state update globally across the application
            WeakReferenceMessenger.Default.Send(new OrderChangedMessage());
        }
        catch (Exception ex)
        {
            ChefStatusMessage = $"❌ Error advancing status: {ex.Message}";
        }
    }
}