using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsKitchen.Infrastructure.Config;
using FastTechFoodsKitchen.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace FastTechFoodsKitchen.Infrastructure.Extensions
{
    public static class RabbitMQExtensions
    {
        public static IServiceCollection AddRabbitMQConfiguration(this IServiceCollection services)
        {
            // Configura o RabbitMQ nativo
            var connectionString = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_STRING") 
                ?? "amqp://guest:guest@localhost:5672";
            
            var rabbitMQSettings = new RabbitMQSettings()
            {
                ConnectionString = connectionString,
                Queues = new RabbitMQSettings.QueueSettings()
            };

            services.AddSingleton(rabbitMQSettings);

            // Registra o factory do RabbitMQ
            services.AddSingleton<IConnectionFactory>(sp =>
            {
                var uri = new Uri(connectionString);
                return new ConnectionFactory()
                {
                    Uri = uri,
                    // DispatchConsumersAsync foi removido na versão 7.x
                };
            });

            // Registra o connection como singleton
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = sp.GetRequiredService<IConnectionFactory>();
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            // TODO: Registrar MessagePublisher quando os DTOs estiverem prontos
            services.AddScoped<IMessagePublisher, MessagePublisher>();

            return services;
        }
    }
}
