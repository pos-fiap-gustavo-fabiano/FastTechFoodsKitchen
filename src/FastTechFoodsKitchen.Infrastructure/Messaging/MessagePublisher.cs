using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsKitchen.Infrastructure.Config;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using FastTechFoodsOrder.Shared.Integration.Messages;
using System.Diagnostics;

namespace FastTechFoodsKitchen.Infrastructure.Messaging
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly IConnection _connection;
        private readonly ILogger<MessagePublisher> _logger;
        private static readonly ActivitySource ActivitySource = new("FastTechFoodsKitchen.MessagePublisher");

        public MessagePublisher(RabbitMQSettings rabbitMQSettings, IConnection connection, ILogger<MessagePublisher> logger)
        {
            _rabbitMQSettings = rabbitMQSettings;
            _connection = connection;
            _logger = logger;
        }

        public async Task PublishOrderAcceptedAsync(OrderAcceptedMessage orderAccepted)
        {
            await PublishMessageAsync(_rabbitMQSettings.Queues.OrderAccepted, orderAccepted);
            _logger.LogInformation("Order {OrderId} accepted and event published", orderAccepted.OrderId);
        }

        public async Task PublishOrderCancelledAsync(OrderCancelledMessage orderCancelled)
        {
            await PublishMessageAsync(_rabbitMQSettings.Queues.OrderCanceled, orderCancelled);
            _logger.LogInformation("Order {OrderId} cancelled and event published", orderCancelled.OrderId);
        }

        //public async Task PublishOrderCompletedAsync(OrderCompletedMessage orderCompleted)
        //{
        //    await PublishMessageAsync(_rabbitMQSettings.Queues.OrderCompleted, orderCompleted);
        //    _logger.LogInformation("Order {OrderId} completed and event published", orderCompleted.OrderId);
        //}

        public async Task PublishOrderPreparingAsync(OrderPreparingMessage orderPreparing)
        {
            await PublishMessageAsync(_rabbitMQSettings.Queues.OrderPreparing, orderPreparing);
            _logger.LogInformation("Order {OrderId} preparing and event published", orderPreparing.OrderId);
        }

        public async Task PublishOrderReadyAsync(OrderReadyMessage orderReady)
        {
            await PublishMessageAsync(_rabbitMQSettings.Queues.OrderReady, orderReady);
            _logger.LogInformation("Order {OrderId} ready and event published", orderReady.OrderId);
        }

        private async Task PublishMessageAsync<T>(string queueName, T message) where T : class
        {
            using var activity = Activity.Current;
            activity?.SetTag("rabbitmq.queueName", queueName);
            activity?.SetTag("message.type", typeof(T).Name);
            await Task.Run(async () =>
            {
                using var channel = await _connection.CreateChannelAsync();
                
                // Declara a fila se ela não existir
                await channel.QueueDeclareAsync(queue: queueName,
                                   durable: true,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null);

                // Serializa a mensagem
                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    //PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var body = Encoding.UTF8.GetBytes(json);

                // Propriedades da mensagem
                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    Type = typeof(T).Name
                };
                
                // Adiciona trace context para correlação
                if (Activity.Current != null)
                {
                    properties.Headers = new Dictionary<string, object?>
                    {
                        ["trace-id"] = Activity.Current.TraceId.ToString(),
                        ["span-id"] = Activity.Current.SpanId.ToString()
                    };
                }
                
                // Publica a mensagem
                await channel.BasicPublishAsync(exchange: "",
                                   routingKey: queueName,
                                   body: body,
                                   mandatory: false,
                                   basicProperties: properties);

                _logger.LogDebug("Published message to queue {QueueName}: {Message}", queueName, json);
            });
        }
    }
}
