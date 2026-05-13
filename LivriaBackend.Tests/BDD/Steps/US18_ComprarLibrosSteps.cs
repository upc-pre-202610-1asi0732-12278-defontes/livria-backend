// US18 – BDD Step Definitions
// Framework: SpecFlow + xUnit
// Valida el comportamiento del proceso de compra directamente
// sobre los agregados Order, OrderItem y Shipping del dominio.

using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Entities;
using LivriaBackend.commerce.Domain.Model.ValueObjects;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;

namespace LivriaBackend.Tests.BDD.Steps
{
    [Binding]
    public class US18_ComprarLibrosSteps
    {
        // ------------------------------------------------------------------
        // Estado compartido entre pasos
        // ------------------------------------------------------------------
        private int               _userClientId;
        private string            _userEmail     = string.Empty;
        private List<OrderItem>   _cartItems     = new();
        private Order?            _createdOrder;
        private Exception?        _caughtException;

        // ------------------------------------------------------------------
        // Background
        // ------------------------------------------------------------------

        [Given(@"existe un cliente con ID (\d+) y email ""(.*)""")]
        public void US18_DadoExisteCliente(int clientId, string email)
        {
            _userClientId = clientId;
            _userEmail    = email;
        }

        [Given(@"el carrito contiene los siguientes ítems:")]
        public void US18_DadoCarritoConItems(Table table)
        {
            _cartItems = table.Rows.Select(row => new OrderItem(
                bookId:     int.Parse(row["BookId"]),
                bookTitle:  row["Title"],
                bookAuthor: row["Author"],
                bookPrice:  decimal.Parse(row["Price"],
                    System.Globalization.CultureInfo.InvariantCulture),
                bookCover:  "cover.jpg",
                quantity:   int.Parse(row["Quantity"])
            )).ToList();
        }

        // ------------------------------------------------------------------
        // Given compartido
        // ------------------------------------------------------------------

        [Given(@"el lector ha revisado su carrito y decide completar la compra")]
        public void US18_DadoLectorRevisaCarrito()
        {
            Assert.NotEmpty(_cartItems);
        }

        [Given(@"el carrito del lector está vacío")]
        public void US18_DadoCarritoVacio()
        {
            _cartItems = new List<OrderItem>();
        }

        // ------------------------------------------------------------------
        // AC2a – Compra sin delivery
        // ------------------------------------------------------------------

        [When(@"el lector selecciona ""Completar compra"" sin envío a domicilio")]
        public void US18_AC2a_CuandoCompletaCompraSinDelivery()
        {
            _createdOrder = new Order(
                userClientId:  _userClientId,
                userEmail:     _userEmail,
                userPhone:     "999888777",
                userFullName:  "Lector Prueba",
                recipientName: "Lector Prueba",
                isDelivery:    false,
                shipping:      null,
                orderItems:    _cartItems,
                status:        "pending"
            );
        }

        [Then(@"el sistema debe registrar la orden con los ítems del carrito")]
        public void US18_EntoncesOrdenRegistrada()
        {
            Assert.NotNull(_createdOrder);
            Assert.NotEmpty(_createdOrder!.Items);
        }

        [Then(@"el total de la orden debe ser (.*)")]
        public void US18_EntoncesTotalCorrecto(decimal expectedTotal)
        {
            Assert.Equal(expectedTotal, _createdOrder!.Total);
        }

        [Then(@"el estado inicial de la orden debe ser ""(.*)""")]
        public void US18_EntoncesEstadoCorrecto(string expectedStatus)
        {
            Assert.Equal(expectedStatus, _createdOrder!.Status);
        }

        [Then(@"el sistema debe generar un código único de (\d+) caracteres para la orden")]
        public void US18_EntoncesCodigoGenerado(int expectedLength)
        {
            Assert.False(string.IsNullOrWhiteSpace(_createdOrder!.Code));
            Assert.Equal(expectedLength, _createdOrder.Code.Length);
        }

        // ------------------------------------------------------------------
        // AC2b – Compra con delivery
        // ------------------------------------------------------------------

        [When(@"el lector selecciona ""Completar compra"" con envío a ""(.*)""")]
        public void US18_AC2b_CuandoCompletaCompraConDelivery(string district)
        {
            var shipping = new Shipping(
                "Av. Test 123", "Lima", district, "Referencia test");

            _createdOrder = new Order(
                userClientId:  _userClientId,
                userEmail:     _userEmail,
                userPhone:     "999888777",
                userFullName:  "Lector Prueba",
                recipientName: "Lector Prueba",
                isDelivery:    true,
                shipping:      shipping,
                orderItems:    _cartItems,
                status:        "pending"
            );
        }

        [Then(@"el total de la orden debe incluir el costo de envío de (.*)")]
        public void US18_EntoncesTotalIncluyeEnvio(decimal shippingCost)
        {
            Assert.NotNull(_createdOrder!.Shipping);
            Assert.Equal(shippingCost, _createdOrder.Shipping!.Price);
        }

        [Then(@"el total final debe ser (.*)")]
        public void US18_EntoncesTotalFinalCorrecto(decimal expectedTotal)
        {
            Assert.Equal(expectedTotal, _createdOrder!.Total);
        }

        // ------------------------------------------------------------------
        // AC2c – Carrito vacío
        // ------------------------------------------------------------------

        [When(@"el lector intenta seleccionar ""Completar compra""")]
        public void US18_AC2c_CuandoIntentaComprarConCarritoVacio()
        {
            try
            {
                _createdOrder = new Order(
                    userClientId:  _userClientId,
                    userEmail:     _userEmail,
                    userPhone:     "999888777",
                    userFullName:  "Lector Prueba",
                    recipientName: "Lector Prueba",
                    isDelivery:    false,
                    shipping:      null,
                    orderItems:    _cartItems, // vacío
                    status:        "pending"
                );
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [Then(@"el sistema debe rechazar la compra con un error de carrito vacío")]
        public void US18_EntoncesRechazaCarritoVacio()
        {
            Assert.NotNull(_caughtException);
            Assert.IsType<ArgumentException>(_caughtException);
            Assert.Contains("item", _caughtException!.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------
        // AC2d – Delivery sin shipping details
        // ------------------------------------------------------------------

        [When(@"el lector selecciona ""Completar compra"" con delivery pero sin datos de envío")]
        public void US18_AC2d_CuandoCompraDeliverySinShipping()
        {
            try
            {
                _createdOrder = new Order(
                    userClientId:  _userClientId,
                    userEmail:     _userEmail,
                    userPhone:     "999888777",
                    userFullName:  "Lector Prueba",
                    recipientName: "Lector Prueba",
                    isDelivery:    true,
                    shipping:      null, // ← error intencional
                    orderItems:    _cartItems,
                    status:        "pending"
                );
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [Then(@"el sistema debe rechazar la compra con un error de datos de envío")]
        public void US18_EntoncesRechazaDeliverySinShipping()
        {
            Assert.NotNull(_caughtException);
            Assert.IsType<ArgumentException>(_caughtException);
            Assert.Contains("Shipping", _caughtException!.Message,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}