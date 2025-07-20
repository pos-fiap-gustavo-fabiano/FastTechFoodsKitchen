using FastTechFoodsKitchen.Application.DTOs.Analytics;
using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsKitchen.Domain.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FastTechFoodsKitchen.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;
    private readonly IMongoCollection<Order> _ordersCollection;

    public AnalyticsService(IApplicationDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
        _ordersCollection = _context.GetCollection<Order>("orders");
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        try
        {
            var filter = BuildOrderFilter(startDate, endDate, status);
            var orders = await _ordersCollection.Find(filter).ToListAsync();

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.Total);
            var averageTicket = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            var ordersInPreparation = orders.Count(o => o.Status.ToLower() == "preparation" || o.Status.ToLower() == "preparing");

            var ordersByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());

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
            var filter = BuildOrderFilter(startDate, endDate);
            var orders = await _ordersCollection.Find(filter).ToListAsync();

            var totalOrders = orders.Count;
            
            var distribution = orders
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusDistributionDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = totalOrders > 0 ? Math.Round((decimal)g.Count() / totalOrders * 100, 2) : 0
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
            var filter = BuildOrderFilter(startDate, endDate);
            var orders = await _ordersCollection.Find(filter).ToListAsync();

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

    private FilterDefinition<Order> BuildOrderFilter(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
    {
        var builder = Builders<Order>.Filter;
        var filters = new List<FilterDefinition<Order>>();

        if (startDate.HasValue)
            filters.Add(builder.Gte(o => o.OrderDate, startDate.Value));

        if (endDate.HasValue)
            filters.Add(builder.Lte(o => o.OrderDate, endDate.Value));

        if (!string.IsNullOrEmpty(status))
            filters.Add(builder.Eq(o => o.Status, status));

        return filters.Count > 0 ? builder.And(filters) : builder.Empty;
    }
}
