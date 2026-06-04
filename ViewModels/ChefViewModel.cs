using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PizzaApp.ViewModels;

public partial class ChefViewModel : ViewModelBase
{
    private readonly Action _onLogout;

    [ObservableProperty]
    private ObservableCollection<Order> _pendingOrders = new();

    public ChefViewModel(Action onLogout)
    {
        _onLogout = onLogout;
        LoadOrders();
    }

    [RelayCommand]
    private void LoadOrders()
    {
        using (var context = new AppDbContext())
        {
            // Load orders that are Pending, along with their text menu items
            var orders = context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Pending)
                .OrderBy(o => o.OrderDate)
                .ToList();

            PendingOrders = new ObservableCollection<Order>(orders);
        }
    }

    [RelayCommand]
    private void CompleteCooking(Order order)
    {
        using (var context = new AppDbContext())
        {
            var dbOrder = context.Orders.FirstOrDefault(o => o.Id == order.Id);
            if (dbOrder != null)
            {
                // Core automation rule: If it's delivery, send to Driver. If counter, complete it!
                if (dbOrder.Type == OrderType.Delivery)
                {
                    dbOrder.Status = OrderStatus.Ready; // Driver sees this
                }
                else
                {
                    dbOrder.Status = OrderStatus.Completed; // Counter order picked up immediately
                }

                context.SaveChanges();
            }
        }
        LoadOrders(); // Refresh the grid screen
    }

    [RelayCommand]
    private void Logout() => _onLogout.Invoke();
}