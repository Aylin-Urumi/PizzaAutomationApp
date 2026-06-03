namespace PizzaApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;

    public MainWindowViewModel()
    {
        // Default to showing the Login Screen on startup
        _currentPage = new LoginViewModel();
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        // Using the Toolkit's built-in change notification property agent
        set => SetProperty(ref _currentPage, value);
    }
}