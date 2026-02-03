using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Mabean.Interop;
using Mabean.Services;
using Mabean.ViewModels;
using Mabean.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Mabean;

public partial class App : Application
{
    public static IServiceProvider Services { get; set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<KeysManagmentViewModel>();
        services.AddTransient<PayloadManagerViewModel>();
        services.AddTransient<BehaviorSimulationViewModel>();
        services.AddTransient<SimulateBehaviorService>();

        services.AddSingleton<PayloadService>();

        Services = services.BuildServiceProvider();
        LoggerService.Init();
        InteropInit.Init();

        System.Diagnostics.Debug.WriteLine("System Diagnostics Debug");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += (_, _) =>
            {
                LoggerService.Shutdown();
            };
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
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