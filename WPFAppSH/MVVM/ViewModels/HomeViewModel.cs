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

        IsConnected = _deviceClientHandler.IsDeviceConnected;
        ConnectionStatus = _deviceClientHandler.IsDeviceConnected ? "Connected" : "Disconnected";
        
        _deviceClientHandler.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    [ObservableProperty]
    private string _toggleLampButtonContent = "Off";
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
    }
    public bool IsLampOn => _lampService.Lamp.Toggled;

    [RelayCommand(CanExecute = nameof(CanToggleLamp))]
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

    [RelayCommand(CanExecute = nameof(CanToggleLampConnection))]
    private void ToggleLampConnection()
    {
        if (_deviceClientHandler.IsDeviceConnected == true)
        {
            Task.Run(() => _deviceClientHandler.UpdateDeviceTwinDeviceConnectionStateAsync(false));

            _deviceClientHandler.IsDeviceConnected = false;
            _deviceClientHandler.Settings.ConnectionState = false;
            OnConnectionStatusChanged(false);
        }
        
        else if (_deviceClientHandler.IsDeviceConnected == false)
        {
            Task.Run(() => _deviceClientHandler.UpdateDeviceTwinDeviceConnectionStateAsync(true));

            _deviceClientHandler.IsDeviceConnected = true;
            _deviceClientHandler.Settings.ConnectionState = true;
            OnConnectionStatusChanged(true);
        }
    }

    private bool CanToggleLampConnection()
    {
        return _deviceClientHandler.IsConnectionStringSet;
    }

    private bool CanToggleLamp()
    {
        return _deviceClientHandler.IsDeviceConnected;
    }
}
