using System.IO;

namespace Shared;

public class ConnectionStringStorage
{
    private readonly string _filePath;
    public ConnectionStringStorage(string filePath = "connectionString.txt")
    {
        _filePath = filePath;
    }
    public void SaveConnectionString(string connectionString)
    {
        try
        {
            File.WriteAllText(_filePath, connectionString);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    public string LoadConnectionString()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                return File.ReadAllText(_filePath);
            }
            else
            {
                return string.Empty; 
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
