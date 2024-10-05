using System;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Functions
{
    public class ViewIoTMessages
    {
        private readonly ILogger<ViewIoTMessages> _logger;

        public ViewIoTMessages(ILogger<ViewIoTMessages> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ViewIoTMessages))]
        public void Run([EventHubTrigger("iothub-ehub-willejioth-62401525-6c94d1cc2c", Connection = "IoTHubEndpoint")] EventData[] events)
        {
            foreach (EventData @event in events)
            {
                _logger.LogInformation("Event Body: {body}", @event.Body);
                _logger.LogInformation("Event Content-Type: {contentType}", @event.ContentType);
            }
        }
    }
}
