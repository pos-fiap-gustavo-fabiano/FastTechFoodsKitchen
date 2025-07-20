using FastTechFoodsKitchen.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FastTechFoodsKitchen.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém dados analíticos para o dashboard da cozinha
    /// </summary>
    /// <param name="startDate">Data de início do filtro (opcional)</param>
    /// <param name="endDate">Data de fim do filtro (opcional)</param>
    /// <param name="status">Status dos pedidos para filtrar (opcional)</param>
    /// <returns>Dados consolidados do dashboard</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var analytics = await _analyticsService.GetDashboardAnalyticsAsync(startDate, endDate, status);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard analytics");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém a distribuição de pedidos por status
    /// </summary>
    /// <param name="startDate">Data de início do filtro (opcional)</param>
    /// <param name="endDate">Data de fim do filtro (opcional)</param>
    /// <returns>Lista com distribuição de pedidos por status</returns>
    [HttpGet("orders-by-status")]
    public async Task<IActionResult> GetOrdersByStatus(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var distribution = await _analyticsService.GetOrdersByStatusAsync(startDate, endDate);
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém os produtos mais vendidos
    /// </summary>
    /// <param name="top">Número de produtos a retornar (padrão: 5)</param>
    /// <param name="startDate">Data de início do filtro (opcional)</param>
    /// <param name="endDate">Data de fim do filtro (opcional)</param>
    /// <returns>Lista dos produtos mais vendidos</returns>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] int top = 5,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var topProducts = await _analyticsService.GetTopProductsAsync(top, startDate, endDate);
            return Ok(topProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top products");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint para validar o funcionamento das analytics
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "analytics", timestamp = DateTime.UtcNow });
    }
}
