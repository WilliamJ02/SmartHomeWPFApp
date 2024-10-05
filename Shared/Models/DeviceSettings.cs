namespace Shared.Models;

public class DeviceSettings
{
    private string _deviceId = Guid.NewGuid().ToString();
    public bool ConnectionState { get; set; }
    public bool DeviceState { get; set; }
    public string? ConnectionString { get; set; } = null!;
    public string? DeviceName { get; set; } = "Bedroom Lamp";
    public string? DeviceType { get; set; } = "Light";
}

