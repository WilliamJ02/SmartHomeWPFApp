using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Shared.Handlers;

namespace WPFAppSH.MVVM.ViewModels;

public partial class MainWindowModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DeviceClientHandler _deviceClientHandler;

    public MainWindowModel(IServiceProvider serviceProvider, DeviceClientHandler deviceClientHandler)
    {
        _serviceProvider = serviceProvider;
        _deviceClientHandler = deviceClientHandler;

        CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
    }

    private void OnProcessExit(object sender, EventArgs e)
    {
        var disconnectResult = _deviceClientHandler.Disconnect();
        Console.WriteLine(disconnectResult.Message);
    }

    [RelayCommand]
    private void GoToSettings()
    {
        var mainWindowModel = _serviceProvider.GetRequiredService<MainWindowModel>();
        mainWindowModel.CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
    }

    [RelayCommand]
    private void GoToHome()
    {
        var mainWindowModel = _serviceProvider.GetRequiredService<MainWindowModel>();
        mainWindowModel.CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
    }

    [RelayCommand]
    private void ExitApp()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            _deviceClientHandler.Disconnect();           
        };
        Environment.Exit(0);
    }

    [ObservableProperty]
    private ObservableObject _currentViewModel;
}
