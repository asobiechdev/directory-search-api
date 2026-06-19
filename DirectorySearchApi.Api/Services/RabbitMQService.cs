using RabbitMQ.Client;
using System.Text.Json;

namespace DirectorySearchApi.Api.Services;

public interface IRabbitMQService
{
    Task PublishAsync<T>(string exchangeName, string routingKey, T message);
    void Dispose();
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService(ILogger<RabbitMQService> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        _logger.LogInformation("RabbitMQ connected");
    }

    public async Task PublishAsync<T>(string exchangeName, string routingKey, T message)
    {
        try
        {
            // Stwórz exchange (jeśli nie istnieje)
            await _channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            var json = JsonSerializer.Serialize(message);
            var body = System.Text.Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation($"Message published: {exchangeName}/{routingKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error publishing message: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
        _logger.LogInformation("RabbitMQ connection closed");
    }
}