using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using PizzaApp.ViewModels;
using PizzaApp.Views;
using PizzaApp.Data; 

namespace PizzaApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // MERGED: Out with the old blank context block, in with the automatic seeder!
        DatabaseSeeder.Seed();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(), 
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}