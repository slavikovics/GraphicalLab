using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GraphicalLab.Services;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.FigureLoaderService;
using GraphicalLab.Services.FilePickerService;
using GraphicalLab.Services.ObjToFigureConverter;
using GraphicalLab.Services.ToastManagerService;
using GraphicalLab.Services.WritableBitmapProviderService;
using GraphicalLab.Transform;
using Microsoft.Extensions.DependencyInjection;
using GraphicalLab.ViewModels;
using GraphicalLab.Views;

namespace GraphicalLab;

public partial class App : Application
{
    private static ServiceProvider? ServiceProvider { get; set; }

    private void RegisterUserServices(Window topLevel)
    { 
        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton<IToastManager, ToastManager>();
        serviceCollection.AddSingleton<IWritableBitmapProvider, WritableBitmapProvider>();
        serviceCollection.AddSingleton<IDebuggableBitmapControl, DebuggableBitmapControl>();
        serviceCollection.AddSingleton<MainWindowViewModel>();
        serviceCollection.AddSingleton<LinesPageViewModel>();
        serviceCollection.AddSingleton<CirclesPageViewModel>();
        serviceCollection.AddSingleton<CurvesPageViewModel>();
        serviceCollection.AddSingleton<IObjToFigureConverter, ObjToFigureConverter>();
        serviceCollection.AddSingleton<Rotate>();
        serviceCollection.AddSingleton<Move>();
        serviceCollection.AddSingleton<IFilePickerService, FilePickerService>(sp =>
        {
            var window = topLevel;
            return new FilePickerService(window);
        });
        serviceCollection.AddSingleton<IFigureLoader, FigureLoader>();
        serviceCollection.AddSingleton<TransformPageViewModel>();
        ServiceProvider = serviceCollection?.BuildServiceProvider();
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
            desktop.MainWindow = new MainWindow();
            RegisterUserServices(desktop.MainWindow);
            var mainWindowViewModel = ServiceProvider?.GetService<MainWindowViewModel>();
            
            if (mainWindowViewModel != null)
            {
                desktop.MainWindow.DataContext = mainWindowViewModel;
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