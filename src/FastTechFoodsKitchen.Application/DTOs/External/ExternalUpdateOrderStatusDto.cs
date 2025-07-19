namespace FastTechFoodsKitchen.Application.DTOs.External
{
    public class ExternalUpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
    }
}
