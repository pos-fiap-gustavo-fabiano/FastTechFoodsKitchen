using FastTechFoodsKitchen.Application.DTOs.Analytics;
using FastTechFoodsKitchen.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FastTechFoodsKitchen.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(IAnalyticsRepository analyticsRepository, ILogger<AnalyticsService> logger)
    {
        _analyticsRepository = analyticsRepository;
        _logger = logger;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        try
        {
            var totalOrders = await _analyticsRepository.GetTotalOrdersCountAsync(startDate, endDate, status);
            var totalRevenue = await _analyticsRepository.GetTotalRevenueAsync(startDate, endDate, status);
            var averageTicket = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            
            // Para contar os pedidos em preparação, fazemos uma consulta específica
            var ordersInPreparation = await _analyticsRepository.GetTotalOrdersCountAsync(startDate, endDate, "preparation");
            ordersInPreparation += await _analyticsRepository.GetTotalOrdersCountAsync(startDate, endDate, "preparing");

            var ordersByStatus = await _analyticsRepository.GetOrdersByStatusAsync(startDate, endDate);
            var topProducts = await GetTopProductsAsync(5, startDate, endDate);

            return new DashboardAnalyticsDto
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                AverageTicket = averageTicket,
                OrdersInPreparation = ordersInPreparation,
                OrdersByStatus = ordersByStatus,
                TopProducts = topProducts
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard analytics");
            throw;
        }
    }

    public async Task<List<OrderStatusDistributionDto>> GetOrdersByStatusAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var ordersByStatus = await _analyticsRepository.GetOrdersByStatusAsync(startDate, endDate);
            var totalOrders = ordersByStatus.Values.Sum();
            
            var distribution = ordersByStatus
                .Select(kvp => new OrderStatusDistributionDto
                {
                    Status = kvp.Key,
                    Count = kvp.Value,
                    Percentage = totalOrders > 0 ? Math.Round((decimal)kvp.Value / totalOrders * 100, 2) : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return distribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status");
            throw;
        }
    }

    public async Task<List<ProductSalesDto>> GetTopProductsAsync(int top = 5, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var orders = await _analyticsRepository.GetOrdersWithItemsAsync(startDate, endDate);
            var productSales = new Dictionary<string, ProductSalesDto>();

            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    var key = $"{item.ProductId}_{item.Name}";
                    
                    if (productSales.ContainsKey(key))
                    {
                        var existing = productSales[key];
                        productSales[key] = existing with
                        {
                            QuantitySold = existing.QuantitySold + item.Quantity,
                            Revenue = existing.Revenue + (item.UnitPrice * item.Quantity)
                        };
                    }
                    else
                    {
                        productSales[key] = new ProductSalesDto
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Name,
                            QuantitySold = item.Quantity,
                            Revenue = item.UnitPrice * item.Quantity
                        };
                    }
                }
            }

            return productSales.Values
                .OrderByDescending(p => p.QuantitySold)
                .Take(top)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top products");
            throw;
        }
    }
}
