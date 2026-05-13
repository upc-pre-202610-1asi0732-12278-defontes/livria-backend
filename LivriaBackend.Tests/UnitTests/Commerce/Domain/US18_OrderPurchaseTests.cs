// US18 – Core Entity Unit Test
// Valida la lógica de dominio de Order, OrderItem y Shipping
// para el proceso de compra de libros digitales y físicos.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Entities;
using LivriaBackend.commerce.Domain.Model.ValueObjects;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Commerce.Domain
{
    public class US18_OrderPurchaseTests
    {
        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        private static OrderItem BuildItem(
            int     bookId   = 1,
            string  title    = "El Principito",
            string  author   = "Saint-Exupéry",
            decimal price    = 29.99m,
            string  cover    = "cover.jpg",
            int     quantity = 1) =>
            new OrderItem(bookId, title, author, price, cover, quantity);

        private static Order BuildPickupOrder(List<OrderItem> items) =>
            new Order(
                userClientId: 1,
                userEmail:    "lector@livria.com",
                userPhone:    "999888777",
                userFullName: "Lector Prueba",
                recipientName:"Lector Prueba",
                isDelivery:   false,
                shipping:     null,
                orderItems:   items,
                status:       "pending"
            );

        private static Order BuildDeliveryOrder(List<OrderItem> items, Shipping shipping) =>
            new Order(
                userClientId: 1,
                userEmail:    "lector@livria.com",
                userPhone:    "999888777",
                userFullName: "Lector Prueba",
                recipientName:"Lector Prueba",
                isDelivery:   true,
                shipping:     shipping,
                orderItems:   items,
                status:       "pending"
            );

        // ==================================================================
        // OrderItem — cálculo de ItemTotal
        // ==================================================================

        [Fact]
        public void US18_OrderItem_WhenCreated_ShouldCalculateItemTotalCorrectly()
        {
            // Arrange
            const decimal price    = 29.99m;
            const int     quantity = 3;

            // Act
            var item = BuildItem(price: price, quantity: quantity);

            // Assert
            Assert.Equal(price * quantity, item.ItemTotal);
        }

        [Fact]
        public void US18_OrderItem_WhenPriceIsZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BuildItem(price: 0m));
        }

        [Fact]
        public void US18_OrderItem_WhenQuantityIsZero_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BuildItem(quantity: 0));
        }

        [Fact]
        public void US18_OrderItem_WhenTitleIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                BuildItem(title: ""));
        }

        // ==================================================================
        // Shipping — cálculo de precio por zona
        // ==================================================================

        [Theory]
        [InlineData("Miraflores",       5.00)]   // zona 1
        [InlineData("San Isidro",       5.00)]   // zona 1
        [InlineData("Barranco",         8.00)]   // zona 2
        [InlineData("La Molina",        8.00)]   // zona 2
        [InlineData("Comas",           12.00)]   // zona 3
        [InlineData("Villa El Salvador",12.00)]  // zona 3
        public void US18_Shipping_ShouldCalculatePriceByZone(
            string district, decimal expectedPrice)
        {
            // Act
            var price = Shipping.CalculateShippingPrice(district);

            // Assert
            Assert.Equal(expectedPrice, price);
        }

        [Fact]
        public void US18_Shipping_WhenAddressIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new Shipping("", "Lima", "Miraflores", ""));
        }

        // ==================================================================
        // Order — total sin envío (recojo en tienda)
        // ==================================================================

        [Fact]
        public void US18_Order_PickUp_TotalShouldEqualSumOfItemTotals()
        {
            // Arrange
            var items = new List<OrderItem>
            {
                BuildItem(price: 29.99m, quantity: 2),  // 59.98
                BuildItem(price: 39.99m, quantity: 1),  // 39.99
            };
            var expectedTotal = 59.98m + 39.99m;

            // Act
            var order = BuildPickupOrder(items);

            // Assert
            Assert.Equal(expectedTotal, order.Total);
        }

        [Fact]
        public void US18_Order_PickUp_ShouldHavePendingStatus()
        {
            // Arrange
            var items = new List<OrderItem> { BuildItem() };

            // Act
            var order = BuildPickupOrder(items);

            // Assert
            Assert.Equal("pending", order.Status);
        }

        [Fact]
        public void US18_Order_PickUp_ShouldGenerateNonEmptyCode()
        {
            // Arrange
            var items = new List<OrderItem> { BuildItem() };

            // Act
            var order = BuildPickupOrder(items);

            // Assert — el código alfanumérico se genera automáticamente
            Assert.False(string.IsNullOrWhiteSpace(order.Code));
            Assert.Equal(6, order.Code.Length);
        }

        // ==================================================================
        // Order — total CON envío (delivery)
        // ==================================================================

        [Fact]
        public void US18_Order_Delivery_TotalShouldIncludeShippingPrice()
        {
            // Arrange
            var shipping = new Shipping(
                "Av. Larco 123", "Lima", "Miraflores", "Ref: cerca al parque");
            // Miraflores = zona 1 = 5.00

            var items = new List<OrderItem>
            {
                BuildItem(price: 29.99m, quantity: 1),  // 29.99
            };
            var expectedTotal = 29.99m + shipping.Price; // 29.99 + 5.00 = 34.99

            // Act
            var order = BuildDeliveryOrder(items, shipping);

            // Assert
            Assert.Equal(expectedTotal, order.Total);
        }

        [Fact]
        public void US18_Order_Delivery_WhenShippingIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            var items = new List<OrderItem> { BuildItem() };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new Order(
                    userClientId:  1,
                    userEmail:     "lector@livria.com",
                    userPhone:     "999888777",
                    userFullName:  "Lector Prueba",
                    recipientName: "Lector Prueba",
                    isDelivery:    true,
                    shipping:      null,   // ← error: delivery sin shipping
                    orderItems:    items,
                    status:        "pending"
                ));
        }

        // ==================================================================
        // Order — UpdateStatus
        // ==================================================================

        [Theory]
        [InlineData("in progress")]
        [InlineData("delivered")]
        public void US18_Order_UpdateStatus_WhenValidStatus_ShouldChangeStatus(
            string newStatus)
        {
            // Arrange
            var items = new List<OrderItem> { BuildItem() };
            var order = BuildPickupOrder(items);

            // Act
            order.UpdateStatus(newStatus);

            // Assert
            Assert.Equal(newStatus, order.Status);
        }

        [Fact]
        public void US18_Order_UpdateStatus_WhenInvalidStatus_ShouldThrowArgumentException()
        {
            // Arrange
            var items = new List<OrderItem> { BuildItem() };
            var order = BuildPickupOrder(items);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                order.UpdateStatus("pagado")); // no es un estado válido
        }

        [Fact]
        public void US18_Order_WhenNoItems_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyItems = new List<OrderItem>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                BuildPickupOrder(emptyItems));
        }
    }
}