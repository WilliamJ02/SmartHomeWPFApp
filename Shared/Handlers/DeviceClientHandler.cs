using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Shared.Models;
using System.Text;

namespace Shared.Handlers;

public class DeviceClientHandler
{
    public event Action<bool>? ConnectionStatusChanged;
    public bool IsConnectionStringSet {  get; set; } = false;
    public bool IsDeviceConnected { get; set; } = false;
    private DeviceClient? _client;
    private ConnectionStringStorage _connectionStringStorage = new();
    public DeviceSettings Settings { get; private set; } = new DeviceSettings();

    public void SetConnectionString(string connectionString)
    {
        Settings.ConnectionString = connectionString;
    }

    public ResultResponse Initialize(Lamp lamp)
    {
        var response = new ResultResponse();

        try
        {
            _client = DeviceClient.CreateFromConnectionString(Settings.ConnectionString);

            if (_client != null)
            {
                _client.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);
                
                Task.Run(() =>  UpdateDeviceTwinPropertiesAsync(lamp));
                response.Succeeded = true;
                
            }
            else
            {
                response.Succeeded = false;
            }
        }
        catch (Exception ex)
        {
            response.Succeeded = false;
            response.Message = ex.Message;
        }

        return response;
    }
    public ResultResponse Disconnect()
    {
        var response = new ResultResponse();

        try
        {
            Settings.DeviceState = false;

            ConnectionStatusChanged?.Invoke(false);

            Task.Run(UpdateDeviceTwinDeviceStateAsync);
            UpdateDeviceTwinDeviceConnectionStateAsync(false).Wait();

            response.Succeeded = true;
            response.Message = "Device disconnected";

        }
        catch (Exception ex)
        {
            response.Succeeded = false;
            response.Message = ex.Message;
        }

        return response;
    }

    public void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
    {
        bool isConnected = status == ConnectionStatus.Connected;

        if (Settings.DeviceState == false && reason != ConnectionStatusChangeReason.Connection_Ok)
        {
            isConnected = false;
        }

        if (status == ConnectionStatus.Disconnected || status == ConnectionStatus.Disabled)
        {
            isConnected = false;
            Task.Run(() => UpdateDeviceTwinDeviceConnectionStateAsync(false));
        }
        else if (status == ConnectionStatus.Connected)
        {
            isConnected = true;
            Task.Run(() => UpdateDeviceTwinDeviceConnectionStateAsync(true));
        }

        ConnectionStatusChanged?.Invoke(isConnected);
    }

    public MethodResponse GenerateMethodResponse(string message, int statusCode)
    {
        try
        {
            var json = JsonConvert.SerializeObject(new { Message = message });
            var methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(json), statusCode);
            return methodResponse;
        }
        catch (Exception ex)
        {
            var json = JsonConvert.SerializeObject(new { Message = ex.Message });
            var methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(json), statusCode);
            return methodResponse;
        }
    }

    public async Task<ResultResponse> UpdateDeviceTwinDeviceStateAsync()
    {
        var response = new ResultResponse();

        try
        {
            var reportedProperties = new TwinCollection
            {
                ["deviceState"] = Settings.DeviceState
            };

            if (_client != null)
            {
                await _client.UpdateReportedPropertiesAsync(reportedProperties);

                response.Succeeded = true;
            }
            else
            {
                response.Succeeded = false;
                response.Message = "No device was found";
            }
        }
        catch (Exception ex)
        {
            response.Succeeded = false;
            response.Message = ex.Message;
        }

        return response;
    }
    public async Task<ResultResponse> UpdateDeviceTwinDeviceConnectionStateAsync(bool connectionState)
    {
        var response = new ResultResponse();

        try
        {
            var reportedProperties = new TwinCollection
            {
                ["connectionState"] = connectionState
            };

            if (_client != null)
            {
                await _client.UpdateReportedPropertiesAsync(reportedProperties);

                response.Succeeded = true;
                response.Message = $"Device ConnectionState set to {connectionState}";
            }
            else
            {
                response.Succeeded = false;
                response.Message = "No device was found";
            }
        }
        catch (Exception ex)
        {
            response.Succeeded = false;
            response.Message = ex.Message;
        }

        return response;
    }
    public async Task<ResultResponse> UpdateDeviceTwinPropertiesAsync(Lamp lamp)
    {
        var response = new ResultResponse();

        try
        {
            var deviceId = Settings.DeviceId;
            var existingIds = _connectionStringStorage.LoadAllDeviceIds();
            
            var reportedProperties = new TwinCollection
            {
                ["connectionState"] = true,
                ["deviceType"] = Settings.DeviceType,
                ["deviceName"] = Settings.DeviceName,
                ["deviceState"] = lamp.Toggled,
                ["LampToggled"] = lamp.Toggled,
                ["LampBrightness"] = lamp.Brightness
            };

            if (!existingIds.Contains(deviceId))
            {
                reportedProperties["deviceId"] = deviceId;
            }

            if (_client != null)
            {
                await _client.UpdateReportedPropertiesAsync(reportedProperties);
                response.Succeeded = true;
            }
            else
            {
                response.Succeeded = false;
                response.Message = "No device was found";
            }
        }
        catch (Exception ex)
        {
            response.Succeeded = false;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResultResponse> SendDataAsync(string data, CancellationToken cancellationToken)
    {
        var response = new ResultResponse();

        try
        {
            if (_client == null)
            {
                response.Succeeded = false;
                response.Message = "Device client is not initialized.";
                return response;
            }

            var message = new Message(Encoding.UTF8.GetBytes(data));

            await _client.SendEventAsync(message, cancellationToken);

            response.Succeeded = true;
            response.Message = "Data sent successfully.";
        }
        catch (Exception ex)
        {
            response.Succeeded = false;
            response.Message = ex.Message;
        }

        return response;
    }
}

