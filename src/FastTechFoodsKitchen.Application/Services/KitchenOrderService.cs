using System.Diagnostics;
using FastTechFoodsKitchen.Application.DTOs;
using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsKitchen.Domain.Entities;
using FastTechFoodsOrder.Shared.Enums;
using FastTechFoodsOrder.Shared.Integration.Messages;
using FastTechFoodsOrder.Shared.Utils;
using Microsoft.Extensions.Logging;

namespace FastTechFoodsKitchen.Application.Services
{
    public class KitchenOrderService : IKitchenOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<KitchenOrderService> _logger;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ActivitySource _activitySource;
        public KitchenOrderService(
            IOrderRepository orderRepository,
            ILogger<KitchenOrderService> logger,
            IMessagePublisher messagePublisher,
            ActivitySource activitySource
            )
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _messagePublisher = messagePublisher;
            _activitySource = activitySource;
        }

        public async Task ProcessOrderCreatedAsync(OrderCreatedMessage orderCreatedMessage)
        {
            _logger.LogInformation("Processing OrderCreated for Order {OrderId}", orderCreatedMessage.OrderId);

            try
            {
                var currentActivity = Activity.Current;
                if (currentActivity != null)
                {
                    currentActivity.SetTag("order.id", orderCreatedMessage.OrderId);
                    currentActivity.SetTag("operation", "ProcessOrderCreated");
                    currentActivity.SetTag("customer.id", orderCreatedMessage.CustomerId);
                }
                // Verificar se o pedido já existe na cozinha
                var existingOrder = await _orderRepository.GetByOrderIdAsync(orderCreatedMessage.OrderId);
                if (existingOrder != null)
                {
                    _logger.LogWarning("Order {OrderId} already exists in kitchen, skipping", orderCreatedMessage.OrderId);
                    return;
                }

                var kitchenOrder = new Order
                {
                    Id = orderCreatedMessage.OrderId,
                    OrderId = orderCreatedMessage.OrderId,
                    CustomerId = orderCreatedMessage.CustomerId,
                    OrderDate = DateTime.UtcNow, 
                    Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Pending), 
                    Items = orderCreatedMessage.Items?.Select(item => new KitchenOrderItem
                    {
                        ProductId = item.ProductId,
                        Name = item.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice 
                    }).ToList() ?? new List<KitchenOrderItem>(),
                    Total = orderCreatedMessage.Total,
                    CreatedAt = DateTime.UtcNow
                   
                };

                await _orderRepository.CreateAsync(kitchenOrder);

                _logger.LogInformation("Order {OrderId} successfully created in kitchen with {ItemCount} items", 
                    orderCreatedMessage.OrderId, 
                    kitchenOrder.Items.Count);

                // TODO: Implementar lógicas adicionais:
                // 1. Notificar equipe da cozinha
                // 2. Verificar disponibilidade de ingredientes
                // 3. Inicializar processo de preparação
                // 4. Publicar evento OrderAccepted se tudo estiver ok
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderCreated for Order {OrderId}", orderCreatedMessage.OrderId);
                throw;
            }
        }

        public async Task AcceptOrderAsync(string orderId, string acceptedBy, int estimatedTime, string notes = "")
        {
            using var activity = _activitySource.StartActivity("AcceptOrderAsync");
            activity?.SetTag("order.id", orderId);
            
            var existingOrder = await _orderRepository.GetByOrderIdAsync(orderId);
            if (existingOrder == null)
            {
                _logger.LogWarning("Order {OrderId} not found for acceptance", orderId);
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            // Atualizar apenas o status mantendo todos os outros dados
            existingOrder.Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Accepted);
            existingOrder.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(existingOrder);

            await _messagePublisher.PublishOrderAcceptedAsync(new OrderAcceptedMessage()
            { 
                OrderId = orderId,
                EventDate = DateTime.Now,
                Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Accepted),
                UpdatedBy = acceptedBy,
                
            });
        }

        public async Task StartPreparingAsync(string orderId, string startedBy, string notes = "")
        {
            using var activity = _activitySource.StartActivity("StartPreparingAsync");
            activity?.SetTag("order.id", orderId);
            var existingOrder = await _orderRepository.GetByOrderIdAsync(orderId);
            if (existingOrder == null)
            {
                _logger.LogWarning("Order {OrderId} not found for acceptance", orderId);
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            // Atualizar apenas o status mantendo todos os outros dados
            existingOrder.Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Preparing); ;
            existingOrder.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(existingOrder);

            await _messagePublisher.PublishOrderPreparingAsync(new OrderPreparingMessage()
            {
                OrderId = orderId,
                EventDate = DateTime.Now,
                Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Preparing),
                UpdatedBy = startedBy,
            });
        }

        public async Task MarkAsReadyAsync(string orderId, string completedBy, string notes = "")
        {
            using var activity = _activitySource.StartActivity("MarkAsReadyAsync");
            activity?.SetTag("order.id", orderId);
            
            var existingOrder = await _orderRepository.GetByOrderIdAsync(orderId);
            if (existingOrder == null)
            {
                _logger.LogWarning("Order {OrderId} not found for acceptance", orderId);
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            existingOrder.Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Ready); ;
            existingOrder.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(existingOrder);

            await _messagePublisher.PublishOrderReadyAsync(new OrderReadyMessage()
            {
                OrderId = orderId,
                EventDate = DateTime.Now,
                Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Ready),
                UpdatedBy = completedBy,
            });
        }

        public async Task CancelOrderAsync(string orderId, string cancelledBy, string reason, string notes = "")
        {
            var existingOrder = await _orderRepository.GetByOrderIdAsync(orderId);
            if (existingOrder == null)
            {
                _logger.LogWarning("Order {OrderId} not found for acceptance", orderId);
                throw new InvalidOperationException($"Order with ID {orderId} not found");
            }

            existingOrder.Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Cancelled); ;
            existingOrder.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(existingOrder);

            await _messagePublisher.PublishOrderCancelledAsync(new OrderCancelledMessage()
            {
                OrderId = orderId,
                EventDate = DateTime.Now,
                Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Cancelled),
                CancelledBy = cancelledBy,
                CancelReason = reason,
            });
        }

        public Task UpdateOrderStatusAsync(string orderId, OrderStatus newStatus, string updatedBy, string notes = "")
        {
            // TODO: Implementar
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<KitchenOrderDto>> GetOrdersAsync(OrderStatus? status = null)
        {
            try
            {
                IEnumerable<Order> orders;
                
                if (status.HasValue)
                {
                    orders = await _orderRepository.GetByStatusAsync(status.Value.ToString());
                }
                else
                {
                    orders = await _orderRepository.GetAllAsync();
                }

                return orders.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders with status {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<KitchenOrderDto>> GetPendingOrdersAsync()
        {
            try
            {
                var orders = await _orderRepository.GetByStatusAsync("Received");
                return orders.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending orders");
                throw;
            }
        }

        public async Task<KitchenOrderDto?> GetOrderByIdAsync(string orderId)
        {
            try
            {
                var order = await _orderRepository.GetByOrderIdAsync(orderId);
                return order != null ? MapToDto(order) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                throw;
            }
        }

        private static KitchenOrderDto MapToDto(Order order)
        {
            return new KitchenOrderDto
            {
                Id = order.Id,
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Items = order.Items?.Select(item => new KitchenOrderItemDto
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                }).ToList() ?? new List<KitchenOrderItemDto>(),
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}
