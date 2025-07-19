namespace FastTechFoodsKitchen.Infrastructure.Config
{
    public class RabbitMQSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public QueueSettings Queues { get; set; } = new QueueSettings();

        public class QueueSettings
        {
            public string OrderAccepted { get; set; } = "order.accepted.queue";
            public string OrderPreparing { get; set; } = "order.preparing.queue";
            public string OrderReady { get; set; } = "order.ready.queue";
            public string OrderCanceled { get; set; } = "order.canceled.queue";
            public string OrderCompleted { get; set; } = "order.completed.queue";
        }
    }
}
