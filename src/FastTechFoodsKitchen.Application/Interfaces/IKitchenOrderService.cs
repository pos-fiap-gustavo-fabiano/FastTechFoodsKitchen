using FastTechFoodsKitchen.Application.DTOs;
using FastTechFoodsOrder.Shared.Enums;
using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsKitchen.Application.Interfaces
{
    public interface IKitchenOrderService
    {
        Task ProcessOrderCreatedAsync(OrderCreatedMessage orderCreatedMessage);
        Task AcceptOrderAsync(string orderId, string acceptedBy, int estimatedTime, string notes = "");
        Task StartPreparingAsync(string orderId, string startedBy, string notes = "");
        Task MarkAsReadyAsync(string orderId, string completedBy, string notes = "");
        Task CancelOrderAsync(string orderId, string cancelledBy, string reason, string notes = "");
        Task UpdateOrderStatusAsync(string orderId, OrderStatus newStatus, string updatedBy, string notes = "");
        Task<IEnumerable<KitchenOrderDto>> GetOrdersAsync(OrderStatus? status = null);
        Task<IEnumerable<KitchenOrderDto>> GetPendingOrdersAsync();
        Task<KitchenOrderDto?> GetOrderByIdAsync(string orderId);
    }
}
