using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Mabean.Abstract;
using Mabean.Interop;
using Mabean.Services;
using Mabean.ViewModels;
using Mabean.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mabean;

public partial class App : Application
{
    public static IHost Host { get; private set; } = null!;
    public static IServiceProvider Services => Host.Services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<EventsService>();
                services.AddHostedService(sp => sp.GetRequiredService<EventsService>());

                services.AddTransient<IAiService, SemanticKernelService>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<KeysManagmentViewModel>();
                services.AddTransient<PayloadManagerViewModel>();
                services.AddTransient<BehaviorSimulationViewModel>();
                services.AddTransient<EventsViewModel>();

                services.AddSingleton<PayloadService>();
                services.AddTransient<SimulateBehaviorService>();
                
            })
            .Build();

        Host.Start(); 

        LoggerService.Init();
        InteropInit.Init();

        System.Diagnostics.Debug.WriteLine("System Diagnostics Debug");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += async (_, _) =>
            {
                await ShutdownAsync();
            };

            DisableAvaloniaDataAnnotationValidation();

            try
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainWindowViewModel>()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during MainWindow initialization: {ex}");
                Console.Error.WriteLine(ex);
                throw;
            }


        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task ShutdownAsync()
    {
        try
        {
            if (Host != null)
            {
                await Host.StopAsync();
                Host.Dispose();
            }
        }
        finally
        {
            LoggerService.Shutdown();
        }
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators
                .OfType<DataAnnotationsValidationPlugin>()
                .ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
