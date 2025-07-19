using FastTechFoodsOrder.Shared.Enums;
using FastTechFoodsOrder.Shared.Utils;

namespace FastTechFoodsKitchen.Application.Tests.Utils
{
    public class OrderStatusUtilsTests
    {
        [Fact]
        public void ConvertStringToStatus_ValidStatuses_ShouldReturnCorrectEnum()
        {
            // Arrange & Act & Assert
            Assert.Equal(OrderStatus.Pending, OrderStatusUtils.ConvertStringToStatus("pending"));
            Assert.Equal(OrderStatus.Accepted, OrderStatusUtils.ConvertStringToStatus("accepted"));
            Assert.Equal(OrderStatus.Preparing, OrderStatusUtils.ConvertStringToStatus("preparing"));
            Assert.Equal(OrderStatus.Ready, OrderStatusUtils.ConvertStringToStatus("ready"));
            Assert.Equal(OrderStatus.Cancelled, OrderStatusUtils.ConvertStringToStatus("cancelled"));
            Assert.Equal(OrderStatus.Delivered, OrderStatusUtils.ConvertStringToStatus("delivered"));
        }

        [Fact]
        public void ConvertStringToStatus_CaseInsensitive_ShouldWork()
        {
            // Arrange & Act & Assert
            Assert.Equal(OrderStatus.Pending, OrderStatusUtils.ConvertStringToStatus("PENDING"));
            Assert.Equal(OrderStatus.Accepted, OrderStatusUtils.ConvertStringToStatus("Accepted"));
            Assert.Equal(OrderStatus.Preparing, OrderStatusUtils.ConvertStringToStatus("PreParing"));
        }

        [Fact]
        public void ConvertStringToStatus_InvalidStatus_ShouldReturnNull()
        {
            // Arrange & Act & Assert
            Assert.Null(OrderStatusUtils.ConvertStringToStatus("invalid"));
            Assert.Null(OrderStatusUtils.ConvertStringToStatus(""));
            Assert.Null(OrderStatusUtils.ConvertStringToStatus(null));
        }

        [Fact]
        public void ConvertStatusToString_AllStatuses_ShouldReturnLowercase()
        {
            // Arrange & Act & Assert
            Assert.Equal("pending", OrderStatusUtils.ConvertStatusToString(OrderStatus.Pending));
            Assert.Equal("accepted", OrderStatusUtils.ConvertStatusToString(OrderStatus.Accepted));
            Assert.Equal("preparing", OrderStatusUtils.ConvertStatusToString(OrderStatus.Preparing));
            Assert.Equal("ready", OrderStatusUtils.ConvertStatusToString(OrderStatus.Ready));
            Assert.Equal("cancelled", OrderStatusUtils.ConvertStatusToString(OrderStatus.Cancelled));
            Assert.Equal("delivered", OrderStatusUtils.ConvertStatusToString(OrderStatus.Delivered));
        }

        [Fact]
        public void IsValidStatus_ValidStatuses_ShouldReturnTrue()
        {
            // Arrange & Act & Assert
            Assert.True(OrderStatusUtils.IsValidStatus("pending"));
            Assert.True(OrderStatusUtils.IsValidStatus("accepted"));
            Assert.True(OrderStatusUtils.IsValidStatus("PREPARING"));
        }

        [Fact]
        public void IsValidStatus_InvalidStatuses_ShouldReturnFalse()
        {
            // Arrange & Act & Assert
            Assert.False(OrderStatusUtils.IsValidStatus("invalid"));
            Assert.False(OrderStatusUtils.IsValidStatus(""));
            Assert.False(OrderStatusUtils.IsValidStatus(null));
        }

        [Fact]
        public void IsValidStatusTransition_ValidTransitions_ShouldReturnTrue()
        {
            // Arrange & Act & Assert
            Assert.True(OrderStatusUtils.IsValidStatusTransition(OrderStatus.Pending, OrderStatus.Accepted));
            Assert.True(OrderStatusUtils.IsValidStatusTransition(OrderStatus.Accepted, OrderStatus.Preparing));
            Assert.True(OrderStatusUtils.IsValidStatusTransition(OrderStatus.Preparing, OrderStatus.Ready));
            Assert.True(OrderStatusUtils.IsValidStatusTransition(OrderStatus.Ready, OrderStatus.Delivered));
        }

        [Fact]
        public void IsValidStatusTransition_InvalidTransitions_ShouldReturnFalse()
        {
            // Arrange & Act & Assert
            Assert.False(OrderStatusUtils.IsValidStatusTransition(OrderStatus.Pending, OrderStatus.Ready));
            Assert.False(OrderStatusUtils.IsValidStatusTransition(OrderStatus.Cancelled, OrderStatus.Accepted));
            Assert.False(OrderStatusUtils.IsValidStatusTransition(OrderStatus.Delivered, OrderStatus.Preparing));
        }

        [Fact]
        public void GetAllValidStatuses_ShouldReturnAllStatuses()
        {
            // Act
            var statuses = OrderStatusUtils.GetAllValidStatuses().ToList();

            // Assert
            Assert.Contains("pending", statuses);
            Assert.Contains("accepted", statuses);
            Assert.Contains("preparing", statuses);
            Assert.Contains("ready", statuses);
            Assert.Contains("cancelled", statuses);
            Assert.Contains("delivered", statuses);
            Assert.DoesNotContain("unknown", statuses);
        }

        [Fact]
        public void GetStatusDescription_AllStatuses_ShouldReturnDescriptions()
        {
            // Arrange & Act & Assert
            Assert.Equal("Pendente", OrderStatusUtils.GetStatusDescription(OrderStatus.Pending));
            Assert.Equal("Aceito", OrderStatusUtils.GetStatusDescription(OrderStatus.Accepted));
            Assert.Equal("Em Preparação", OrderStatusUtils.GetStatusDescription(OrderStatus.Preparing));
            Assert.Equal("Pronto", OrderStatusUtils.GetStatusDescription(OrderStatus.Ready));
            Assert.Equal("Cancelado", OrderStatusUtils.GetStatusDescription(OrderStatus.Cancelled));
            Assert.Equal("Entregue", OrderStatusUtils.GetStatusDescription(OrderStatus.Delivered));
        }
    }
}
