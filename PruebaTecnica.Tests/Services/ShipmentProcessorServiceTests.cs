using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PruebaTecnica.Models;
using PruebaTecnica.Services;
using Xunit;

namespace PruebaTecnica.Tests.Services;

public class ShipmentProcessorServiceTests
{
    [Fact]
    public async Task ProcessShipmentWebhookAsync_NullPayload_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ShipmentProcessorService>>();
        var service = new ShipmentProcessorService(mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ProcessShipmentWebhookAsync(null!));
    }

    [Fact]
    public async Task ProcessShipmentWebhookAsync_ValidPayload_ProcessesSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ShipmentProcessorService>>();
        var service = new ShipmentProcessorService(mockLogger.Object);
        var payload = new ShipmentWebhookPayload
        {
            EventId = "123",
            ProviderId = "Prov1"
        };

        // Act
        var task = service.ProcessShipmentWebhookAsync(payload);
        
        // Assert
        // We just ensure it runs and completes without throwing (after 2 seconds delay)
        await task;
        Assert.True(task.IsCompletedSuccessfully);
    }
}
