using System;
using System.Linq;
using PizzaApp.Data;
using PizzaApp.Models;

namespace PizzaApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;

    public MainWindowViewModel()
    {
        _currentPage = CreateLoginScreen();
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    private LoginViewModel CreateLoginScreen()
    {
        return new LoginViewModel(role =>
        {
            if (role == "Cashier")
            {
                CurrentPage = new CashierViewModel(() => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Manager")
            {
                CurrentPage = new ManagerViewModel(() => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Chef")
            {
                // ROUTE CHEF
                CurrentPage = new ChefViewModel(() => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Driver" || role == "Delivery")
            {
                // ROUTE DRIVER / DELIVERY
                try
                {
                    using (var context = new AppDbContext())
                    {
                        // Look up the active user profile matching this specific role in the database
                        var activeDriver = context.Users.FirstOrDefault(u => u.Role == role);
                        
                        if (activeDriver != null)
                        {
                            // Route directly using the found user data
                            CurrentPage = new DeliveryViewModel(activeDriver, () => CurrentPage = CreateLoginScreen());
                        }
                        else
                        {
                            // Fallback safety catch: Create a baseline profile instance if the db entry is somehow missing
                            var fallbackDriver = new User { Username = role, Role = role };
                            CurrentPage = new DeliveryViewModel(fallbackDriver, () => CurrentPage = CreateLoginScreen());
                        }
                    }
                }
                catch (Exception)
                {
                    // Master fallback if database connection encounters an unexpected issue
                    var fallbackDriver = new User { Username = "Delivery Personnel", Role = "Driver" };
                    CurrentPage = new DeliveryViewModel(fallbackDriver, () => CurrentPage = CreateLoginScreen());
                }
            }
        });
    }
}