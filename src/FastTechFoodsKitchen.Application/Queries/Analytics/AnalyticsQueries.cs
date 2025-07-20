using FastTechFoodsKitchen.Application.DTOs.Analytics;

namespace FastTechFoodsKitchen.Application.Queries.Analytics;

public record GetDashboardAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Status = null
);

public record GetOrdersByStatusQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record GetTopProductsQuery(
    int Top = 5,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
