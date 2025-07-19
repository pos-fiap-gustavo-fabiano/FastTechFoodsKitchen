using FastTechFoodsKitchen.Application.DTOs.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace FastTechFoodsKitchen.Infrastructure.Services
{
    public interface IOrdersApiClient
    {
        Task<List<ExternalOrderDto>> GetOrdersAsync(string? customerId = null);
        Task<ExternalOrderDto?> GetOrderByIdAsync(string orderId);
        Task<bool> UpdateOrderStatusAsync(string orderId, ExternalUpdateOrderStatusDto updateDto);
    }

    public class OrdersApiClient : IOrdersApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrdersApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public OrdersApiClient(HttpClient httpClient, ILogger<OrdersApiClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Configurar a base URL da API de Orders
            var baseUrl = "http://localhost:5043";
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<List<ExternalOrderDto>> GetOrdersAsync(string? customerId = null)
        {
            try
            {
                var url = "/api/orders";
                if (!string.IsNullOrEmpty(customerId))
                {
                    url += $"?customerId={Uri.EscapeDataString(customerId)}";
                }

                _logger.LogInformation("Fetching orders from external API: {Url}", url);
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var orders = JsonSerializer.Deserialize<List<ExternalOrderDto>>(content, _jsonOptions);
                    return orders ?? new List<ExternalOrderDto>();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch orders. Status: {StatusCode}", response.StatusCode);
                    return new List<ExternalOrderDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders from external API");
                return new List<ExternalOrderDto>();
            }
        }

        public async Task<ExternalOrderDto?> GetOrderByIdAsync(string orderId)
        {
            try
            {
                var url = $"/api/orders/{Uri.EscapeDataString(orderId)}";
                
                _logger.LogInformation("Fetching order {OrderId} from external API", orderId);
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var order = JsonSerializer.Deserialize<ExternalOrderDto>(content, _jsonOptions);
                    return order;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Order {OrderId} not found in external API", orderId);
                    return null;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch order {OrderId}. Status: {StatusCode}", orderId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId} from external API", orderId);
                return null;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, ExternalUpdateOrderStatusDto updateDto)
        {
            try
            {
                var url = $"/api/orders/{Uri.EscapeDataString(orderId)}/status";
                
                _logger.LogInformation("Updating order {OrderId} status to {Status} in external API", orderId, updateDto.Status);
                
                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PatchAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated order {OrderId} status to {Status}", orderId, updateDto.Status);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update order {OrderId} status. Status: {StatusCode}", orderId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status in external API", orderId);
                return false;
            }
        }
    }
}
