using System;

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
            else if (role == "Driver")
            {
                // ROUTE DRIVER
                CurrentPage = new DeliveryViewModel(() => CurrentPage = CreateLoginScreen());
            }
        });
    }
}