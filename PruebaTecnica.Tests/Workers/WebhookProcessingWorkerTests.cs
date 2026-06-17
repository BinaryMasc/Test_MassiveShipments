using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Workers;
using RabbitMQ.Client;
using Xunit;

namespace PruebaTecnica.Tests.Workers;

public class WebhookProcessingWorkerTests
{
    [Fact]
    public async Task StartAsync_ConnectsToRabbitMQ()
    {
        // Arrange
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IChannel>();
        
        mockConnectionFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockConnection.Object);
            
        mockConnection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockChannel.Object);

        var mockProcessor = new Mock<IShipmentProcessorService>();
        var mockLogger = new Mock<ILogger<WebhookProcessingWorker>>();
        var worker = new WebhookProcessingWorker(mockConnectionFactory.Object, mockProcessor.Object, mockLogger.Object);

        var cts = new CancellationTokenSource();
        
        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        mockConnectionFactory.Verify(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockConnection.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        
        await worker.StopAsync(cts.Token);
    }
}
