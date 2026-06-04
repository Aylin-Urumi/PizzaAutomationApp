using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PizzaApp.Data;
using System;
using System.Linq;

namespace PizzaApp.ViewModels;

public partial class ManagerViewModel : ViewModelBase
{
    private readonly Action _onLogout;

    [ObservableProperty] private decimal _dailyRevenue;
    [ObservableProperty] private int _dailyOrderCount;
    [ObservableProperty] private decimal _monthlyRevenue;
    [ObservableProperty] private int _monthlyOrderCount;

    public ManagerViewModel(Action onLogout)
    {
        _onLogout = onLogout;
        LoadBusinessMetrics();
    }

    private void LoadBusinessMetrics()
    {
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        using (var context = new AppDbContext())
        {
            // 1. Gather Daily Analytics
            DailyOrderCount = context.Orders.Count(o => o.OrderDate.Date == today);
            DailyRevenue = context.Orders
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0.00m;

            // 2. Gather Monthly Analytics
            MonthlyOrderCount = context.Orders.Count(o => o.OrderDate >= firstDayOfMonth);
            MonthlyRevenue = context.Orders
                .Where(o => o.OrderDate >= firstDayOfMonth)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0.00m;
        }
    }

    [RelayCommand]
    private void Refresh() => LoadBusinessMetrics();

    [RelayCommand]
    private void Logout() => _onLogout.Invoke();
}