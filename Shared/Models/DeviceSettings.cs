namespace Shared.Models;

public class DeviceSettings
{   
    public string DeviceId { get; set; } = Guid.NewGuid().ToString();
    public bool DeviceState { get; set; }
    public string? ConnectionString { get; set; } = null!;
    public string? DeviceName { get; set; } = "Bedroom Lamp";
    public string? DeviceType { get; set; } = "Light";
    
    private bool _connectionState;
    public event Action<bool>? ConnectionStateChanged;

    public bool ConnectionState
    {
        get => _connectionState;
        set
        {
            if (_connectionState != value)
            {
                _connectionState = value;
                ConnectionStateChanged?.Invoke(_connectionState);  
            }
        }
    }
}

