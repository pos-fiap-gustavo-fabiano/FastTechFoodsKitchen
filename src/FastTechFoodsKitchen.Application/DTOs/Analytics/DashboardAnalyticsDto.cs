namespace FastTechFoodsKitchen.Application.DTOs.Analytics;

public record DashboardAnalyticsDto
{
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AverageTicket { get; init; }
    public int OrdersInPreparation { get; init; }
    public Dictionary<string, int> OrdersByStatus { get; init; } = new();
    public List<ProductSalesDto> TopProducts { get; init; } = new();
}

public record ProductSalesDto
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int QuantitySold { get; init; }
    public decimal Revenue { get; init; }
}

public record OrderStatusDistributionDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record AnalyticsFilterDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Status { get; init; }
    public int? CustomerId { get; init; }
}
