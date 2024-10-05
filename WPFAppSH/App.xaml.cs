using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using WPFAppSH.MVVM.ViewModels;
using WPFAppSH.MVVM.Views;
using Shared.Services;
using Shared.Handlers;

namespace WPFAppSH;

public partial class App : Application
{
    private static IHost? host;
    public App()
    {
        host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddSingleton<ILampService, LampService>();          

            services.AddSingleton<DeviceClientHandler>(provider =>
            {
                var connectionString = "Your Azure IoT Hub Connection String";
                var deviceClientHandler = new DeviceClientHandler();
                deviceClientHandler.SetConnectionString(connectionString);
                return deviceClientHandler;
            });

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowModel>();

            services.AddSingleton<HomeView>();
            services.AddSingleton<HomeViewModel>();

            services.AddSingleton<SettingsView>();
            services.AddSingleton<SettingsViewModel>();

        }).Build();
    }
    protected override async void OnStartup(StartupEventArgs e)
    {
        var mainWindow = host!.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        using var cts = new CancellationTokenSource();

        try
        {
            await host!.RunAsync();
        }
        catch { }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var deviceClientHandler = host.Services.GetRequiredService<DeviceClientHandler>();
        deviceClientHandler.Disconnect();

        base.OnExit(e);
    }
}
