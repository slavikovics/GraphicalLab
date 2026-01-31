using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using GraphicalLab.Services.ToastManagerService;
using GraphicalLab.Services.WritableBitmapProviderService;
using Microsoft.Extensions.DependencyInjection;
using GraphicalLab.ViewModels;
using GraphicalLab.Views;

namespace GraphicalLab;

public partial class App : Application
{
    private static ServiceProvider? ServiceProvider { get; set; }

    private void RegisterUserServices()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton<IToastManager, ToastManager>();
        serviceCollection.AddSingleton<IWritableBitmapProvider, WritableBitmapProvider>();
        serviceCollection.AddSingleton<MainWindowViewModel>();
        serviceCollection.AddSingleton<LinesPageViewModel>();
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            RegisterUserServices();
            var mainWindowViewModel = ServiceProvider?.GetService<MainWindowViewModel>();
            if (mainWindowViewModel != null)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}