using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared;
using Shared.Handlers;
using Shared.Services;

namespace WPFAppSH.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILampService _lampService;
    private readonly DeviceClientHandler _deviceClientHandler;
    private readonly ConnectionStringStorage _connectionStringStorage;
    public SettingsViewModel(ILampService lampService, DeviceClientHandler deviceClientHandler)
    {
        _lampService = lampService;
        _deviceClientHandler = deviceClientHandler;
        _connectionStringStorage = new ConnectionStringStorage();
    }

    [ObservableProperty]
    private string _connectionStringInput;
    [ObservableProperty]
    private string _connectionSucceededInput;

    [RelayCommand]
    private void SaveConnectionString()
    {
        if (!string.IsNullOrEmpty(ConnectionStringInput))
        {
            var existingConnections = _connectionStringStorage.LoadAllConnectionStringsAndDeviceIds();
            bool connectionExists = existingConnections.Any(pair => pair.connectionString == ConnectionStringInput);

            if (connectionExists)
            {
                var existingDeviceId = existingConnections.First(pair => pair.connectionString == ConnectionStringInput).deviceId;
                _deviceClientHandler.Settings.DeviceId = existingDeviceId;
            }
            else
            {
                _deviceClientHandler.Settings.DeviceId = Guid.NewGuid().ToString();
            }

            _deviceClientHandler.SetConnectionString(ConnectionStringInput);

            var lamp = _lampService.Lamp;

            var result = _deviceClientHandler.Initialize(lamp);

            if (result.Succeeded)
            {
                ConnectionSucceededInput = "Connection successful!";

                if (!connectionExists)
                {
                    _connectionStringStorage.SaveConnectionStringAndDeviceId(ConnectionStringInput, _deviceClientHandler.Settings.DeviceId);
                }

                _deviceClientHandler.IsConnectionStringSet = true;
                _deviceClientHandler.IsDeviceConnected = true;
            }
            else
            {
                ConnectionSucceededInput = "Connection string not found.";
            }
        }
        else
        {
            ConnectionSucceededInput = "Field is empty!";
        }

        ConnectionStringInput = string.Empty;
    }
}
