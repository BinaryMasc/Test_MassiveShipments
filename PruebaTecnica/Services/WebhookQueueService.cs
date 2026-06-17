using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using PruebaTecnica.Interfaces;
using PruebaTecnica.Models;

namespace PruebaTecnica.Services;

public class WebhookQueueService : IWebhookQueueService
{
    private readonly IConnectionFactory _connectionFactory;
    private const string QueueName = "shipments_queue";

    public WebhookQueueService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async ValueTask EnqueueAsync(ShipmentWebhookPayload payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        // BasicProperties is now handled via BasicProperties class or passing null depending on v7 api
        var props = new BasicProperties
        {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }
}
