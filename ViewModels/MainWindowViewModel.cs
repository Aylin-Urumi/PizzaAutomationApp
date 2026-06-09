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
        return new LoginViewModel(authenticatedUser =>
        {
            if (authenticatedUser == null) return;

            string role = authenticatedUser.Role;

            if (role == "Cashier")
            {
                // 🌟 UPDATED: Passing the real authenticated user to Cashier
                CurrentPage = new CashierViewModel(authenticatedUser, () => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Manager")
            {
                CurrentPage = new ManagerViewModel(authenticatedUser, () => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Chef")
            {
                // 🌟 UPDATED: Passing the real authenticated user to Chef
                CurrentPage = new ChefViewModel(authenticatedUser, () => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Driver" || role == "Delivery")
            {
                CurrentPage = new DeliveryViewModel(authenticatedUser, () => CurrentPage = CreateLoginScreen());
            }
        });
    }
}