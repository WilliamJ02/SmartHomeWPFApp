using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AzureFunctions.Functions;

public class AzureDevice
{
    private readonly ILogger _logger;
    private readonly DeviceClient client = DeviceClient.CreateFromConnectionString("HostName=willejiothub.azure-devices.net;DeviceId=006def4e-c6ee-46d1-953d-a37d6d7663f8;SharedAccessKey=3PmBpRdL2MZKH/ftdkWYgX2l/VXk9jEXfUVubXVjkH8=");

    public AzureDevice(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AzureDevice>();
    }

    //[Function("AzureDevice")]
    //public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
    //{
    //    var json = JsonConvert.SerializeObject(new DataMessage());
    //    await SendDataAsync(json);

    //    _logger.LogInformation($"Message sent: {json}");
    //}

    public async Task SendDataAsync(string content)
    {
        using var message = new Message(Encoding.UTF8.GetBytes(content))
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        await client.SendEventAsync(message);
    }

    public class DataMessage
    {
        public string Status { get; set; } = "on";
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
