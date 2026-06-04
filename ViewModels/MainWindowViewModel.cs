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
            // If a Cashier logs in, send them straight to the custom Cashier POS screen!
            if (role == "Cashier")
            {
                CurrentPage = new CashierViewModel(() => 
                {
                    // If they click log out, bring them back to the login screen
                    CurrentPage = CreateLoginScreen();
                });
            }
            else
            {
                // Fallback placeholder for other roles (Manager, Chef, Driver) for now
                CurrentPage = new DashboardViewModel(role, () => {
                    CurrentPage = CreateLoginScreen();
                });
            }
        });
    }
}