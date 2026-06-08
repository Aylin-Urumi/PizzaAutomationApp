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
                CurrentPage = new CashierViewModel(() => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Manager")
            {
                CurrentPage = new ManagerViewModel(() => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Chef")
            {
                CurrentPage = new ChefViewModel(() => CurrentPage = CreateLoginScreen());
            }
            else if (role == "Driver" || role == "Delivery")
            {
                CurrentPage = new DeliveryViewModel(authenticatedUser, () => CurrentPage = CreateLoginScreen());
            }
        });
    }
}