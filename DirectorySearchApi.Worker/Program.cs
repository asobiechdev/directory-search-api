using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

// Połączenie do RabbitMQ
var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

Console.WriteLine("Worker connected to RabbitMQ");

// Deklaruj exchange
await channel.ExchangeDeclareAsync(
    exchange: "contacts.exchange",
    type: ExchangeType.Direct,
    durable: true,
    autoDelete: false);

// Deklaruj queue
await channel.QueueDeclareAsync(
    queue: "search.query",
    durable: true,
    exclusive: false,
    autoDelete: false);

// Bind queue do exchange'a
await channel.QueueBindAsync(
    queue: "search.query",
    exchange: "contacts.exchange",
    routingKey: "search.query");

Console.WriteLine("Queue bound to exchange. Waiting for messages...");

// Consumer — nasłuchuj wiadomości
var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    Console.WriteLine($"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Message received:");
    Console.WriteLine(message);

    // Parse JSON
    try
    {
        var json = JsonDocument.Parse(message);
        var query = json.RootElement.GetProperty("Query").GetString();
        Console.WriteLine($"Query: {query}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing message: {ex.Message}");
    }

    // Acknowledge — powiedz RabbitMQ, że wiadomość się przetworzył
    await channel.BasicAckAsync(ea.DeliveryTag, false);
    Console.WriteLine("Message acknowledged");
};

// Zacznij konsumować
await channel.BasicConsumeAsync(
    queue: "search.query",
    autoAck: false,
    consumerTag: "search-consumer",
    noLocal: false,
    exclusive: false,
    arguments: null,
    consumer: consumer);

Console.WriteLine("Press Enter to stop...");
Console.ReadLine();

// Cleanup
await channel.CloseAsync();
await connection.CloseAsync();