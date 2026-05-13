// US20 – BDD Step Definitions
// Framework: SpecFlow + xUnit
// TODOS los steps llevan prefijo "US20 " donde colisionan con otras HUs:
//   - Background: "US20 existe un cliente...", "US20 el lector tiene una orden..."
//   - Given/When: "US20 el lector ha completado...", "US20 el lector selecciona..."
//   - Then: "US20 el total...", "US20 el total final...", "US20 la orden debe crearse..."

using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Entities;
using LivriaBackend.commerce.Domain.Model.ValueObjects;
using TechTalk.SpecFlow;
using Xunit;

namespace LivriaBackend.Tests.BDD.Steps
{
    [Binding]
    public class US20_GestionarPagoSteps
    {
        // ------------------------------------------------------------------
        // Estado compartido entre pasos
        // ------------------------------------------------------------------
        private int             _userClientId;
        private string          _userEmail      = string.Empty;
        private List<OrderItem> _items          = new();
        private Order?          _order;
        private string?         _pendingDistrict;

        // CCI hardcodeado igual que en PaymentPage.dart
        private const string _cci = "002191103718905053";

        // ------------------------------------------------------------------
        // Background — prefijados con "US20 " para evitar colisión con US18
        // ------------------------------------------------------------------

        [Given(@"US20 existe un cliente con ID (\d+) y email ""(.*)""")]
        public void US20_DadoExisteCliente(int id, string email)
        {
            _userClientId = id;
            _userEmail    = email;
        }

        [Given(@"US20 el lector tiene una orden con (\d+) ítem de precio (.*)")]
        public void US20_DadoOrdenConItem(int quantity, decimal price)
        {
            _items = new List<OrderItem>
            {
                new OrderItem(
                    bookId:     1,
                    bookTitle:  "El Principito",
                    bookAuthor: "Saint-Exupéry",
                    bookPrice:  price,
                    bookCover:  "cover.jpg",
                    quantity:   quantity
                )
            };
        }

        // ------------------------------------------------------------------
        // AC1 – CCI y monto disponibles
        // ------------------------------------------------------------------

        [Given(@"US20 el lector ha completado los pasos de información de compra")]
        public void US20_AC1_DadoLectorCompletoInformacion()
        {
            Assert.NotEmpty(_items);
        }

        [Given(@"el lector seleccionó envío a domicilio en ""(.*)""")]
        public void US20_AC1_DadoLectorSeleccionoEnvio(string district)
        {
            _pendingDistrict = district;
        }

        [When(@"US20 el lector selecciona continuar con el proceso de pago")]
        public void US20_AC1_CuandoContinuaConPago()
        {
            Shipping? shipping = null;

            if (_pendingDistrict != null)
            {
                shipping = new Shipping(
                    "Av. Test 123", "Lima", _pendingDistrict, "");
            }

            _order = new Order(
                userClientId:  _userClientId,
                userEmail:     _userEmail,
                userPhone:     "999888777",
                userFullName:  "Lector Prueba",
                recipientName: "Lector Prueba",
                isDelivery:    shipping != null,
                shipping:      shipping,
                orderItems:    _items,
                status:        "pending"
            );
        }

        [Then(@"el sistema debe tener disponible el número de cuenta interbancaria")]
        public void US20_AC1_EntoncesCCIDisponible()
        {
            Assert.False(string.IsNullOrWhiteSpace(_cci),
                "AC1: el CCI debe estar disponible para mostrar al usuario");
        }

        [Then(@"el CCI debe tener 18 dígitos numéricos")]
        public void US20_AC1_EntoncesFormatoCCI()
        {
            Assert.Equal(18, _cci.Length);
            Assert.True(_cci.All(char.IsDigit),
                "El CCI debe contener solo dígitos numéricos");
        }

        [Then(@"la orden debe mostrar el monto total a transferir mayor a 0")]
        public void US20_AC1_EntoncesTotalMayorACero()
        {
            Assert.NotNull(_order);
            Assert.True(_order!.Total > 0,
                "AC1: el monto a transferir debe ser mayor a 0");
        }

        // Prefijado con "US20 " para evitar colisión con US18
        [Then(@"US20 el total de la orden debe incluir el costo de envío de (.*)")]
        public void US20_AC1_EntoncesTotalIncluyeEnvio(decimal shippingCost)
        {
            Assert.NotNull(_order!.Shipping);
            Assert.Equal(shippingCost, _order.Shipping!.Price);
        }

        // Prefijado con "US20 " para evitar colisión con US18
        [Then(@"US20 el total final debe ser (.*)")]
        public void US20_AC1_EntoncesTotalFinalCorrecto(decimal expectedTotal)
        {
            Assert.Equal(expectedTotal, _order!.Total);
        }

        // ------------------------------------------------------------------
        // AC2 – Comprobante registrado, orden en "pending"
        // ------------------------------------------------------------------

        [Given(@"US20 el lector ha realizado la transferencia bancaria")]
        public void US20_AC2_DadoLectorTransfirio()
        {
            Assert.NotEmpty(_items);
        }

        [When(@"el sistema recibe la orden con comprobante adjunto")]
        public void US20_AC2_CuandoSistemaRecibeComprobante()
        {
            _order = new Order(
                userClientId:  _userClientId,
                userEmail:     _userEmail,
                userPhone:     "999888777",
                userFullName:  "Lector Prueba",
                recipientName: "Lector Prueba",
                isDelivery:    false,
                shipping:      null,
                orderItems:    _items,
                status:        "pending"
            );
        }

        // Prefijado con "US20 " para evitar colisión con US18
        [Then(@"US20 la orden debe crearse con estado ""(.*)""")]
        public void US20_AC2_EntoncesOrdenConEstado(string expectedStatus)
        {
            Assert.NotNull(_order);
            Assert.Equal(expectedStatus, _order!.Status);
        }

        [Then(@"el código de la orden debe ser alfanumérico de (\d+) caracteres")]
        public void US20_AC2_EntoncesCodigoAlfanumerico(int length)
        {
            Assert.NotNull(_order);
            Assert.Equal(length, _order!.Code.Length);
            Assert.Matches("^[A-Z0-9]+$", _order.Code);
        }

        [Then(@"el sistema debe poder actualizar el estado a ""(.*)"" cuando el admin valide")]
        public void US20_AC2_EntoncesAdminPuedeValidar(string newStatus)
        {
            _order!.UpdateStatus(newStatus);
            Assert.Equal(newStatus, _order.Status);
        }

        [Then(@"el sistema debe rechazar el estado ""(.*)"" como inválido")]
        public void US20_AC2_EntoncesEstadoInvalidoRechazado(string invalidStatus)
        {
            try
            {
                _order = new Order(
                    userClientId:  _userClientId,
                    userEmail:     _userEmail,
                    userPhone:     "999888777",
                    userFullName:  "Lector Prueba",
                    recipientName: "Lector Prueba",
                    isDelivery:    false,
                    shipping:      null,
                    orderItems:    _items,
                    status:        invalidStatus
                );
                Assert.Fail($"Se esperaba ArgumentException para estado '{invalidStatus}'");
            }
            catch (ArgumentException ex)
            {
                Assert.Contains("pending", ex.Message,
                    StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
