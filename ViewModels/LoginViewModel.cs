using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PizzaApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaApp.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly Action<string> _onLoginSuccess;

    // The items that will populate our dropdown list
    public List<string> Roles { get; } = new() { "Manager", "Cashier", "Chef", "Driver" };

    [ObservableProperty]
    private string? _selectedRole;

    [ObservableProperty]
    private string _pinCode = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(Action<string> onLoginSuccess)
    {
        _onLoginSuccess = onLoginSuccess;
        // Default select 'Manager' when the screen first loads
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
            // CRITICAL: Checks that BOTH the selected role AND the PIN code match together
            var matchedUser = context.Users.FirstOrDefault(u => u.Role == SelectedRole && u.PinCode == PinCode);

            if (matchedUser != null)
            {
                // Trigger our main frame to switch screens!
                _onLoginSuccess?.Invoke(matchedUser.Role);
            }
            else
            {
                ErrorMessage = $"Access Denied: Invalid PIN for {SelectedRole}.";
            }
        }
    }
}