using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PizzaApp.ViewModels;

public partial class DeliveryViewModel : ViewModelBase
{
    private readonly Action _onLogout;

    [ObservableProperty]
    private ObservableCollection<Order> _deliveryOrders = new();

    public DeliveryViewModel(Action onLogout)
    {
        _onLogout = onLogout;
        LoadDeliveries();
    }

    [RelayCommand]
    private void LoadDeliveries()
    {
        using (var context = new AppDbContext())
        {
            var orders = context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.Type == OrderType.Delivery && o.Status == OrderStatus.Ready)
                .OrderBy(o => o.OrderDate)
                .ToList();

            DeliveryOrders = new ObservableCollection<Order>(orders);
        }
    }

    [RelayCommand]
    private void MarkAsDelivered(Order order)
    {
        using (var context = new AppDbContext())
        {
            var dbOrder = context.Orders.FirstOrDefault(o => o.Id == order.Id);
            if (dbOrder != null)
            {
                dbOrder.Status = OrderStatus.Completed; // Finalizes the lifetime loop
                context.SaveChanges();
            }
        }
        LoadDeliveries();
    }

    [RelayCommand]
    private void Logout() => _onLogout.Invoke();
}