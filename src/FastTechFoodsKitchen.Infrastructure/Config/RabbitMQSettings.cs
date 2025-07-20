using FastTechFoodsOrder.Shared.Constants;

namespace FastTechFoodsKitchen.Infrastructure.Config
{
    public class RabbitMQSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public QueueSettings Queues { get; set; } = new QueueSettings();

        public class QueueSettings
        {
            public string OrderAccepted { get; set; } = QueueNames.OrderAccepted;
            public string OrderPreparing { get; set; } = QueueNames.OrderPreparing;
            public string OrderReady { get; set; } = QueueNames.OrderReady;
            public string OrderCanceled { get; set; } = QueueNames.OrderCancelled;
            public string OrderCompleted { get; set; } = QueueNames.OrderCompleted;
        }
    }
}
