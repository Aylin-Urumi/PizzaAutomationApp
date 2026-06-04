using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PizzaApp.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly Action _onLogout;

    [ObservableProperty]
    private string _roleTitle;

    public DashboardViewModel(string role, Action onLogout)
    {
        RoleTitle = $"{role} Workspace Dashboard";
        _onLogout = onLogout;
    }

    [RelayCommand]
    private void Logout()
    {
        _onLogout?.Invoke();
    }
}