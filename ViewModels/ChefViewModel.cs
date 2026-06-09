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

public partial class ChefViewModel : ViewModelBase, IRecipient<OrderChangedMessage>
{
    private readonly Action _onLogoutAction;

    // 🌟 NEW: Track exactly which chef profile is active on this monitor
    [ObservableProperty]
    private User _currentChef;

    [ObservableProperty]
    private ObservableCollection<Order> _cookingOrders = new();

    [ObservableProperty]
    private string _chefStatusMessage = string.Empty;

    // 🌟 UPDATED: Constructor now accepts the authenticated Chef user
    public ChefViewModel(User authenticatedChef, Action onLogout)
    {
        _currentChef = authenticatedChef;
        _onLogoutAction = onLogout;

        // Load initial cooking screen orders
        LoadChefOrders();

        // Explicitly registers this kitchen instance to the messenger channel bus
        WeakReferenceMessenger.Default.Register<OrderChangedMessage>(this);
    }

    // Required Method for real-time kitchen syncs
    public void Receive(OrderChangedMessage message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadChefOrders());
    }

    [RelayCommand]
    private void Logout()
    {
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
                // 🌟 CRITICAL FILTER: Fetch only orders assigned to THIS specific chef ID
                var ordersList = context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Where(o => o.AssignedChefId == CurrentChef.Id && 
                               (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Cooking))
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