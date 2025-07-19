
using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsKitchen.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishOrderAcceptedAsync(OrderAcceptedMessage orderAccepted);
        Task PublishOrderCancelledAsync(OrderCancelledMessage orderCancelled);
        Task PublishOrderPreparingAsync(OrderPreparingMessage orderPreparing);
        Task PublishOrderReadyAsync(OrderReadyMessage orderReady);
    }
}