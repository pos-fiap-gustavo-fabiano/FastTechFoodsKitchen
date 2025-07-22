using FastTechFoodsOrder.Shared.Integration.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FastTechFoodsKitchen.Api.Consumers
{
    public class OrderUserCancelledMessageConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly ILogger<OrderUserCancelledMessageConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IChannel? _channel;

        public OrderUserCancelledMessageConsumer(
            IConnection connection,
            ILogger<OrderUserCancelledMessageConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _connection = connection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderUserCancelledMessageConsumer started");

            _channel = await _connection.CreateChannelAsync();

            // Declara a fila
            await _channel.QueueDeclareAsync(
                queue: "order.user.cancelled.queue",
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
                    _logger.LogInformation("Received OrderUserCancelledMessage: {Message}", message);

                    var orderUserCancelledMessage = JsonSerializer.Deserialize<OrderCancelledMessage>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (orderUserCancelledMessage != null)
                    {
                        await ProcessOrderUserCancelledMessageAsync(orderUserCancelledMessage);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("OrderUserCancelledMessage processed successfully for Order {OrderId}", orderUserCancelledMessage.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize OrderUserCancelledMessage");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing OrderUserCancelledMessage");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            // Inicia o consumo
            await _channel.BasicConsumeAsync(
                queue: "order.user.cancelled.queue",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("OrderUserCancelledMessageConsumer is listening for messages on queue: order.user.cancelled.queue");

            // Mantém o consumer ativo
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessOrderUserCancelledMessageAsync(OrderCancelledMessage orderUserCancelledMessage)
        {
            using var scope = _serviceProvider.CreateScope();
            
            var handler = scope.ServiceProvider.GetRequiredService<IOrderUserCancelledMessageHandler>();
            await handler.HandleAsync(orderUserCancelledMessage);
        }

        public override void Dispose()
        {
            _channel?.CloseAsync();
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
