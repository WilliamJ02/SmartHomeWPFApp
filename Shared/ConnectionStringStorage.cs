using System.IO;

namespace Shared;

public class ConnectionStringStorage
{
    private readonly string _filePath;

    public ConnectionStringStorage(string filePath = "ConnectionStringsAndIds.txt")
    {
        _filePath = filePath;
    }


    public void SaveConnectionStringAndDeviceId(string connectionString, string deviceId)
    {
        try
        {
            var existingConnections = LoadAllConnectionStringsAndDeviceIds();

            bool connectionExists = existingConnections.Any(pair => pair.connectionString == connectionString);

            if (!connectionExists)
            {
                File.AppendAllLines(_filePath, new[] { connectionString, deviceId });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving connection string and device ID: {ex.Message}");
        }
    }

    public List<(string connectionString, string deviceId)> LoadAllConnectionStringsAndDeviceIds()
    {
        var connectionStringsAndIds = new List<(string connectionString, string deviceId)>();

        try
        {
            if (File.Exists(_filePath))
            {
                var lines = File.ReadAllLines(_filePath);

                for (int i = 0; i < lines.Length; i += 2)
                {
                    if (i + 1 < lines.Length)
                    {
                        connectionStringsAndIds.Add((lines[i], lines[i + 1]));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading connection strings and device IDs: {ex.Message}");
        }

        return connectionStringsAndIds;
    }

    public List<string> LoadAllDeviceIds()
    {
        var deviceIds = new List<string>();

        try
        {
            if (File.Exists(_filePath))
            {
                var lines = File.ReadAllLines(_filePath);

                for (int i = 1; i < lines.Length; i += 2)
                {
                    deviceIds.Add(lines[i]);  
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading device IDs: {ex.Message}");
        }

        return deviceIds;
    }
}
