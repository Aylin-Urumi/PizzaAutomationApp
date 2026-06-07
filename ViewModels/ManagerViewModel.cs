using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.Models;

namespace PizzaApp.ViewModels;

public partial class ManagerViewModel : ViewModelBase
{
    private Action? _onLogoutAction;

    // Analytics Properties
    [ObservableProperty] private decimal _totalSales;
    [ObservableProperty] private decimal _dailyRevenue;
    [ObservableProperty] private decimal _monthlyRevenue;

    [ObservableProperty] private int _totalOrdersCount;
    [ObservableProperty] private ObservableCollection<Order> _recentOrders = new();
    // Inventory Management Properties
    [ObservableProperty] private ObservableCollection<Ingredient> _ingredients = new();
    [ObservableProperty] private Ingredient? _selectedIngredient;
    [ObservableProperty] private string _restockAmountText = string.Empty;

    // Staff Management Properties
    [ObservableProperty] private ObservableCollection<User> _workers = new();
    [ObservableProperty] private string _newWorkerUsername = string.Empty;
    [ObservableProperty] private string _newWorkerPassword = string.Empty;
    [ObservableProperty] private string _newWorkerRole = "Cashier"; // Default selection

    public string[] AvailableRoles => new[] { "Cashier", "Chef", "Driver" };

    // Customer Feedback Properties
    [ObservableProperty] private ObservableCollection<CustomerFeedback> _feedbacks = new();

    [ObservableProperty] private string _managerStatusMessage = string.Empty;

    public ManagerViewModel(Action onLogout)
    {
        _onLogoutAction = onLogout;
        LoadDashboardData();
    }

    [RelayCommand]
    private void Logout() => _onLogoutAction?.Invoke();

    [RelayCommand]
    public void LoadDashboardData()
    {
        using (var context = new AppDbContext())
        {
            // 1. Load Analytics
            var ordersList = context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            RecentOrders = new ObservableCollection<Order>(ordersList);
            TotalSales = ordersList.Sum(o => o.TotalAmount);
            TotalOrdersCount = ordersList.Count;

            // Get current date parameters for processing daily and monthly sales
            var today = DateTime.Today;

            // Calculate Daily Revenue (Matching today's exact date stamp)
            DailyRevenue = ordersList
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => o.TotalAmount);

            // Calculate Monthly Revenue (Matching current month and current year)
            MonthlyRevenue = ordersList
                .Where(o => o.OrderDate.Month == today.Month && o.OrderDate.Year == today.Year)
                .Sum(o => o.TotalAmount);

            // 2. Load Staff Roster (Excluding the Manager to prevent accidental self-deletion)
            var usersList = context.Users.Where(u => u.Role != "Manager").ToList();
            Workers = new ObservableCollection<User>(usersList);

            // 3. Load Customer Feedbacks
            var feedbackList = context.CustomerFeedbacks.OrderByDescending(f => f.DateSubmitted).ToList();
            Feedbacks = new ObservableCollection<CustomerFeedback>(feedbackList);

            // 4. NEW: Load Current Warehouse Inventory Stock Logs
            var inventoryList = context.Ingredients.OrderBy(i => i.Name).ToList();
            Ingredients = new ObservableCollection<Ingredient>(inventoryList);
        }
    }

    [RelayCommand]
    private void AddWorker()
    {
        ManagerStatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(NewWorkerUsername) || string.IsNullOrWhiteSpace(NewWorkerPassword))
        {
            ManagerStatusMessage = "❌ Username and Password PIN cannot be blank!";
            return;
        }

        try
        {
            using (var context = new AppDbContext())
            {
                // Verify username uniqueness
                if (context.Users.Any(u => u.Username.ToLower() == NewWorkerUsername.Trim().ToLower()))
                {
                    ManagerStatusMessage = "❌ An employee with that username already exists!";
                    return;
                }

                var newUser = new User
                {
                    Username = NewWorkerUsername.Trim(),
                    PinCode = NewWorkerPassword.Trim(),
                    Role = NewWorkerRole
                };

                context.Users.Add(newUser);
                context.SaveChanges();
            }

            // Flush inputs and refresh lists
            NewWorkerUsername = string.Empty;
            NewWorkerPassword = string.Empty;
            LoadDashboardData();
            ManagerStatusMessage = "🎉 New employee login registered successfully!";
        }
        catch (Exception ex)
        {
            ManagerStatusMessage = $"❌ Error adding staff: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DeleteWorker(User? worker)
    {
        if (worker == null) return;
        ManagerStatusMessage = string.Empty;

        try
        {
            using (var context = new AppDbContext())
            {
                var dbUser = context.Users.FirstOrDefault(u => u.Id == worker.Id);
                if (dbUser != null)
                {
                    context.Users.Remove(dbUser);
                    context.SaveChanges();
                }
            }

            LoadDashboardData();
            ManagerStatusMessage = $"🗑️ Account '{worker.Username}' removed from the system.";
        }
        catch (Exception ex)
        {
            ManagerStatusMessage = $"❌ Error deleting staff: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DeleteFeedback(CustomerFeedback? feedback)
    {
        if (feedback == null) return;

        try
        {
            using (var context = new AppDbContext())
            {
                var dbFeedback = context.CustomerFeedbacks.FirstOrDefault(f => f.Id == feedback.Id);
                if (dbFeedback != null)
                {
                    context.CustomerFeedbacks.Remove(dbFeedback);
                    context.SaveChanges();
                }
            }

            LoadDashboardData();
            ManagerStatusMessage = "🗑️ Feedback review entry removed from logs.";
        }
        catch (Exception ex)
        {
            ManagerStatusMessage = $"❌ Error clearing feedback: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RestockIngredient()
    {
        ManagerStatusMessage = string.Empty;

        if (SelectedIngredient == null)
        {
            ManagerStatusMessage = "❌ Please choose an ingredient item to restock!";
            return;
        }

        if (!double.TryParse(RestockAmountText, out double supplyValue) || supplyValue <= 0)
        {
            ManagerStatusMessage = "❌ Please enter a valid restock amount greater than 0.";
            return;
        }

        try
        {
            using (var context = new AppDbContext())
            {
                var dbIngredient = context.Ingredients.FirstOrDefault(i => i.Id == SelectedIngredient.Id);
                if (dbIngredient != null)
                {
                    dbIngredient.StockQuantity += supplyValue;
                    context.SaveChanges();
                }
            }

            // Flush panel inputs and refresh list views
            RestockAmountText = string.Empty;
            LoadDashboardData();
            ManagerStatusMessage = $"🎉 Successfully restocked {SelectedIngredient.Name}!";
        }
        catch (Exception ex)
        {
            ManagerStatusMessage = $"❌ Restock transaction failed: {ex.Message}";
        }
    }
}