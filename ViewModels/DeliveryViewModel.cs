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

// FIXED: Added the IRecipient interface implementation to satisfy the compiler and handle real-time sync messages
public partial class DeliveryViewModel : ViewModelBase, IRecipient<OrderChangedMessage>
{
    private readonly Action _onLogout;
    
    // Tracks exactly which delivery worker is logged into this console session
    [ObservableProperty]
    private User _currentDeliveryWorker;

    [ObservableProperty]
    private ObservableCollection<Order> _myDeliveries = new();

    [ObservableProperty]
    private string _deliveryStatusMessage = string.Empty;

    // Constructor accepts the logged-in user profile from MainWindowViewModel
    public DeliveryViewModel(User authenticatedWorker, Action onLogout)
    {
        _currentDeliveryWorker = authenticatedWorker;
        _onLogout = onLogout;
        
        // Load initial runs
        LoadMyDeliveries();

        // FIXED: Explicitly registers this class instance to the messenger channel bus
        WeakReferenceMessenger.Default.Register<OrderChangedMessage>(this);
    }

    // REQUIRED METHOD: This automatically fires when an OrderChangedMessage is broadcasted anywhere in the app
    public void Receive(OrderChangedMessage message)
    {
        // Forces Avalonia's UI thread to cleanly re-fetch the database items safely
        Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadMyDeliveries());
    }

    [RelayCommand]
    private void Logout()
    {
        // Clean up messenger registrations on logout to prevent memory leaks
        WeakReferenceMessenger.Default.UnregisterAll(this);
        _onLogout.Invoke();
    }

    [RelayCommand]
    public void LoadMyDeliveries()
    {
        try
        {
            using (var context = new AppDbContext())
            {
                // Fetch only delivery orders assigned to THIS worker's ID that are not completed yet
                var deliveryList = context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Where(o => o.Type == OrderType.Delivery && 
                                o.AssignedDriverId == CurrentDeliveryWorker.Id && 
                                o.Status != OrderStatus.Completed)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                MyDeliveries = new ObservableCollection<Order>(deliveryList);
            }
        }
        catch (Exception ex)
        {
            DeliveryStatusMessage = $"❌ Error loading runs: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AdvanceOrderStatus(Order order)
    {
        if (order == null) return;
        DeliveryStatusMessage = string.Empty;

        try
        {
            using (var context = new AppDbContext())
            {
                var dbOrder = context.Orders.FirstOrDefault(o => o.Id == order.Id);
                if (dbOrder != null)
                {
                    // Delivery Workflow State Machine
                    if (dbOrder.Status == OrderStatus.Pending || dbOrder.Status == OrderStatus.Cooking || dbOrder.Status == OrderStatus.Ready)
                    {
                        dbOrder.Status = OrderStatus.OutForDelivery;
                        DeliveryStatusMessage = $"🚚 Order #{order.Id} is now out for delivery!";
                    }
                    else if (dbOrder.Status == OrderStatus.OutForDelivery)
                    {
                        dbOrder.Status = OrderStatus.Completed;
                        DeliveryStatusMessage = $"🏁 Order #{order.Id} completed and marked delivered!";
                    }

                    context.SaveChanges();
                }
            }
            
            // Broadcast the state update globally across the application
            WeakReferenceMessenger.Default.Send(new OrderChangedMessage());
        }
        catch (Exception ex)
        {
            DeliveryStatusMessage = $"❌ Error updating status: {ex.Message}";
        }
    }
}