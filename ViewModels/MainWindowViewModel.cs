
namespace PizzaApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;

    public MainWindowViewModel()
    {
        // Boot up into the login screen and pass it a method to run on success
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
            // When login succeeds, swap out the page to a dashboard view!
            CurrentPage = new DashboardViewModel(role, () =>
            {
                // When they click logout inside the dashboard, loop back here
                CurrentPage = CreateLoginScreen();
            });
        });
    }
}