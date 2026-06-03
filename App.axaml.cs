using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using PizzaApp.ViewModels;
using PizzaApp.Views;
using PizzaApp.Data; // 1. Added this to talk to your Data folder

namespace PizzaApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 2. This ensures the 'pizza.db' file is created and seeded before the UI shows up
        using (var context = new AppDbContext())
        {
            context.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(), // Kept your original ViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}