using FastTechFoodsOrder.Shared.Integration.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public class OrderCreatedMessageConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly ILogger<OrderCreatedMessageConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IChannel? _channel;

        public OrderCreatedMessageConsumer(
            IConnection connection,
            ILogger<OrderCreatedMessageConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _connection = connection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCreatedMessageConsumer started");

            _channel = await _connection.CreateChannelAsync();

            // Declara a fila
            await _channel.QueueDeclareAsync(
                queue: "order.created.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Configura o consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received OrderCreatedMessage: {Message}", message);

                    var orderCreatedMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (orderCreatedMessage != null)
                    {
                        await ProcessOrderCreatedMessageAsync(orderCreatedMessage);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("OrderCreatedMessage processed successfully for Order {OrderId}", orderCreatedMessage.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize OrderCreatedMessage");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing OrderCreatedMessage");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            // Inicia o consumo
            await _channel.BasicConsumeAsync(
                queue: "order.created.queue",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("OrderCreatedMessageConsumer is listening for messages");

            // Mantém o consumer ativo
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessOrderCreatedMessageAsync(OrderCreatedMessage orderCreatedMessage)
        {
            using var scope = _serviceProvider.CreateScope();
            
            var handler = scope.ServiceProvider.GetRequiredService<IOrderCreatedMessageHandler>();
            await handler.HandleAsync(orderCreatedMessage);
        }

        public override void Dispose()
        {
            _channel?.CloseAsync();
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
