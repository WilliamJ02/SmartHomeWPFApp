using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Services;
using Shared.Handlers;
using Microsoft.Azure.Devices.Client;

namespace WPFAppSH.MVVM.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DeviceClientHandler _deviceClientHandler;
    private readonly ILampService _lampService;
    private CancellationTokenSource _debounceCancellationTokenSource;

    public HomeViewModel(ILampService lampService, DeviceClientHandler deviceClientHandler)
    {
        _lampService = lampService;
        _deviceClientHandler = deviceClientHandler;

        IsConnected = _deviceClientHandler.Settings.ConnectionState;
        ToggleLampButtonContent = "Off";
        _deviceClientHandler.ConnectionStatusChanged += OnConnectionStatusChanged;
        ConnectionStatus = _deviceClientHandler.Settings.ConnectionState ? "Connected" : "Disconnected";
    }

    [ObservableProperty]
    private string _toggleLampButtonContent;
    
    [ObservableProperty]
    private int _lampBrightness;
    [ObservableProperty]
    private string _connectionStatus;
    [ObservableProperty]
    private bool _isConnected;
    private void OnConnectionStatusChanged(bool isConnected)
    {
        IsConnected = isConnected;
        ConnectionStatus = isConnected ? "Connected" : "Disconnected";

        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(ConnectionStatus));
    }
    public bool IsLampOn => _lampService.Lamp.Toggled;

    [RelayCommand]
    private async Task ToggleLamp()
    {
        var cts = new CancellationTokenSource();

        try
        {
            _lampService.ToggleLamp();
            ToggleLampButtonContent = _lampService.Lamp.Toggled ? "On" : "Off";

            OnPropertyChanged(nameof(IsLampOn));

            var lamp = _lampService.Lamp;

            var twinUpdateResult = await _deviceClientHandler.UpdateDeviceTwinPropertiesAsync(lamp);

            string command = lamp.Toggled ? "ON" : "OFF";
            var sendResult = await _deviceClientHandler.SendDataAsync(command, cts.Token); 
        }
        catch (OperationCanceledException)
        {

        }
        
        catch (Exception ex)
        {
            
        }
        
        finally
        {
            cts.Dispose();
        }
    }

    partial void OnLampBrightnessChanged(int value)
    {
        _lampService.Lamp.Brightness = value;
        DebounceBrightnessUpdate();
    }

    private void DebounceBrightnessUpdate()
    {
        _debounceCancellationTokenSource?.Cancel();

        _debounceCancellationTokenSource = new CancellationTokenSource();

        Task.Delay(500, _debounceCancellationTokenSource.Token)
            .ContinueWith(async task =>
            {
                if (!task.IsCanceled)
                {
                    await SendBrightnessToHubAsync();
                }
            }, TaskScheduler.Default);
    }
    public async Task SendBrightnessToHubAsync()
    {
        var twinUpdateResult = await _deviceClientHandler.UpdateDeviceTwinPropertiesAsync(_lampService.Lamp);
    }

    [RelayCommand]
    private void ToggleLampConnection()
    {
        if (_deviceClientHandler.Settings.ConnectionState)
        {
            Task.Run(() =>
            {
                _deviceClientHandler.Disconnect();
            });

            _deviceClientHandler.Settings.DeviceState = true;
            OnPropertyChanged(nameof(ConnectionStatus));
        }
        else
        {
            Task.Run(() =>
            {
                var lamp = _lampService.Lamp;  
                _deviceClientHandler.Initialize(lamp);  
            });

            _deviceClientHandler.Settings.ConnectionState = true;
            OnPropertyChanged(nameof(ConnectionStatus));
        }
    }
}
