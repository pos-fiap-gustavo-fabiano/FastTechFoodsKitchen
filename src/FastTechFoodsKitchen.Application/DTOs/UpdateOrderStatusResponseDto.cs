namespace FastTechFoodsKitchen.Application.DTOs
{
    public class UpdateOrderStatusResponseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
