using FastTechFoodsKitchen.Application.DTOs;
using FastTechFoodsKitchen.Application.DTOs.External;
using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsOrder.Shared.Enums;
using FastTechFoodsOrder.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastTechFoodsKitchen.Api.Controllers
{
    /// <summary>
    /// Controller para gerenciar pedidos da cozinha.
    /// Busca dados diretamente do banco de dados local da cozinha em vez da API externa.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IKitchenOrderService _kitchenOrderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IKitchenOrderService kitchenOrderService,
            ILogger<OrdersController> logger)
        {
            _kitchenOrderService = kitchenOrderService;
            _logger = logger;
        }

        /// <summary>
        /// Buscar pedidos da cozinha armazenados no banco de dados
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrders([FromQuery] string? status = null)
        {
            try
            {
                IEnumerable<KitchenOrderDto> orders;

                if (!string.IsNullOrEmpty(status))
                {
                    var statusEnum = OrderStatusUtils.ConvertStringToStatus(status);
                    if (statusEnum == null)
                    {
                        return BadRequest(new { error = $"Invalid status: {status}. Valid statuses: {string.Join(", ", OrderStatusUtils.GetAllValidStatuses())}" });
                    }
                    orders = await _kitchenOrderService.GetOrdersAsync(statusEnum);
                }
                else
                {
                    orders = await _kitchenOrderService.GetOrdersAsync();
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders from kitchen database");
                return StatusCode(500, new { error = "Internal server error while getting orders" });
            }
        }

        /// <summary>
        /// Buscar pedido específico da cozinha pelo OrderId
        /// </summary>
        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            try
            {
                var order = await _kitchenOrderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { error = $"Order {orderId} not found in kitchen" });
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId} from kitchen database", orderId);
                return StatusCode(500, new { error = "Internal server error while getting order" });
            }
        }

        /// <summary>
        /// Buscar apenas pedidos pendentes (Received) da cozinha
        /// </summary>
        [HttpGet("pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingOrders()
        {
            try
            {
                var orders = await _kitchenOrderService.GetPendingOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending orders from kitchen database");
                return StatusCode(500, new { error = "Internal server error while getting pending orders" });
            }
        }

        
        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Employee,Admin, Manager")]
        public async Task<IActionResult> UpdateOrderStatusKitchen(string orderId, [FromBody] UpdateOrderStatusDto request)
        {
            try
            {
                if (String.IsNullOrEmpty(orderId))
                {
                    return BadRequest(new UpdateOrderStatusResponseDto
                    {
                        Success = false,
                        Message = "Invalid order ID",
                        OrderId = orderId
                    });
                }

                var status = OrderStatusUtils.ConvertStringToStatus(request.Status);
                if (status == null)
                {
                    return BadRequest(new UpdateOrderStatusResponseDto
                    {
                        Success = false,
                        Message = $"Invalid status: {request.Status}. Valid statuses: {string.Join(", ", OrderStatusUtils.GetAllValidStatuses())}",
                        OrderId = orderId
                    });
                }

                var userName = request.UserName ?? "Operador Cozinha";
                var notes = request.Notes ?? $"Status atualizado via cozinha para {request.Status}";

                // 1. Atualizar na API externa
                var externalUpdateDto = new ExternalUpdateOrderStatusDto
                {
                    Status = request.Status,
                    UpdatedBy = userName,
                    CancelReason = status == OrderStatus.Cancelled ? "Cancelado pela cozinha" : null
                };

               
                try
                {
                    switch (status.Value)
                    {
                        case OrderStatus.Accepted:
                            await _kitchenOrderService.AcceptOrderAsync(orderId, userName, 30, notes);
                            break;
                        case OrderStatus.Preparing:
                            await _kitchenOrderService.StartPreparingAsync(orderId, userName, notes);
                            break;
                        case OrderStatus.Ready:
                            await _kitchenOrderService.MarkAsReadyAsync(orderId, userName, notes);
                            break;
                        case OrderStatus.Cancelled:
                            await _kitchenOrderService.CancelOrderAsync(orderId, userName, notes);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to publish kitchen events for order {OrderId}, but external API was updated", orderId);
                    // Não retorna erro porque a API externa foi atualizada com sucesso
                }

                return Ok(new UpdateOrderStatusResponseDto
                {
                    Success = true,
                    Message = "Order status updated successfully",
                    OrderId = orderId,
                    NewStatus = request.Status.ToLower()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {OrderId} to {Status}", orderId, request.Status);
                return StatusCode(500, new UpdateOrderStatusResponseDto
                {
                    Success = false,
                    Message = "Internal server error while updating order status",
                    OrderId = orderId
                });
            }
        }
    }
}