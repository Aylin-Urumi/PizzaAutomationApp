using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PizzaApp.Data;
using PizzaApp.Models; // 🌟 ADDED this to see the User model
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaApp.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    // 🌟 CHANGED: Now passes the entire User object instead of just a string role
    private readonly Action<User> _onLoginSuccess;

    public List<string> Roles { get; } = new() { "Manager", "Cashier", "Chef", "Driver" };

    [ObservableProperty]
    private string? _selectedRole;

    [ObservableProperty]
    private string _pinCode = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // 🌟 CHANGED: Constructor now expects Action<User>
    public LoginViewModel(Action<User> onLoginSuccess)
    {
        _onLoginSuccess = onLoginSuccess;
        SelectedRole = Roles[0];
    }

    [RelayCommand]
    private void Login()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrEmpty(SelectedRole))
        {
            ErrorMessage = "Please select your staff role.";
            return;
        }

        if (string.IsNullOrWhiteSpace(PinCode) || PinCode.Length < 4)
        {
            ErrorMessage = "Please enter your 4-digit PIN.";
            return;
        }

        using (var context = new AppDbContext())
        {
            var matchedUser = context.Users.FirstOrDefault(u => u.Role == SelectedRole && u.PinCode == PinCode);

            if (matchedUser != null)
            {
                // 🌟 CHANGED: Pass the actual matched user profile over to the main frame!
                _onLoginSuccess?.Invoke(matchedUser);
            }
            else
            {
                ErrorMessage = $"Access Denied: Invalid PIN for {SelectedRole}.";
            }
        }
    }
}