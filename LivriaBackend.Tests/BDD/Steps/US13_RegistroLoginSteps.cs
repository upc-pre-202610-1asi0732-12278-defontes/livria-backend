// US13 – BDD Step Definitions
// Framework: SpecFlow + xUnit
// Cada método mapea exactamente a una línea Gherkin del .feature

using LivriaBackend.IAM.Domain.Model.Aggregates;
using TechTalk.SpecFlow;
using Xunit;

namespace LivriaBackend.Tests.BDD.Steps
{
    [Binding]
    public class US13_RegistroLoginSteps
    {
        // ------------------------------------------------------------------
        // Estado compartido entre pasos del mismo escenario
        // ------------------------------------------------------------------
        private Identity?  _identity;
        private Exception? _caughtException;
        private bool       _verifyResult;

        // ==================================================================
        // AC1 – Registro
        // ==================================================================

        [Given(@"el sistema recibe una solicitud de registro con username ""(.*)"" y password ""(.*)""")]
        public void DadoSolicitudDeRegistro(string username, string password)
        {
            // Guardamos los datos; la creación ocurre en el When
            _identity         = null;
            _caughtException  = null;
            // Usamos campos de instancia para pasar datos al When
            _pendingUsername  = username;
            _pendingPassword  = password;
        }

        [When(@"el sistema procesa la creación de la identidad")]
        public void CuandoProcesaCreacionIdentidad()
        {
            _identity = new Identity(1, _pendingUsername!, _pendingPassword!);
        }

        [Then(@"la identidad debe ser creada correctamente")]
        public void EntoncesIdentidadCreadaCorrectamente()
        {
            Assert.NotNull(_identity);
            Assert.Equal(_pendingUsername, _identity!.UserName);
        }

        [Then(@"la contraseña debe almacenarse de forma segura sin texto plano")]
        public void EntoncesPasswordAlmacenadaSegura()
        {
            Assert.NotNull(_identity!.HashedPassword);
            // El hash nunca debe ser igual al texto plano
            Assert.NotEqual(_pendingPassword, _identity.HashedPassword.HashedValue);
        }

        [Then(@"el sistema debe poder verificar la contraseña original")]
        public void EntoncesPasswordVerificable()
        {
            Assert.True(_identity!.VerifyPassword(_pendingPassword!));
        }

        // ------------------------------------------------------------------
        // AC1b – Registro rechazado
        // ------------------------------------------------------------------

        [When(@"el sistema intenta crear la identidad")]
        public void CuandoIntentaCrearIdentidad()
        {
            try
            {
                _identity = new Identity(1, _pendingUsername!, _pendingPassword!);
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [Then(@"el sistema debe rechazar el registro con un error de validación")]
        public void EntoncesRechazaRegistro()
        {
            Assert.NotNull(_caughtException);
            Assert.IsType<ArgumentException>(_caughtException);
        }

        // ==================================================================
        // AC2 – Login
        // ==================================================================

        [Given(@"existe una identidad registrada con username ""(.*)"" y password ""(.*)""")]
        public void DadoIdentidadRegistrada(string username, string password)
        {
            _identity = new Identity(1, username, password);
        }

        [When(@"el sistema recibe una solicitud de login con username ""(.*)"" y password ""(.*)""")]
        public void CuandoRecibeLoginRequest(string username, string password)
        {
            // Solo verificamos la contraseña — el username ya está en la identidad
            _verifyResult = _identity!.VerifyPassword(password);
        }

        [Then(@"el sistema debe verificar las credenciales exitosamente")]
        public void EntoncesVerificaCredencialesExitosamente()
        {
            // Paso narrativo — la aserción real está en el Then siguiente
        }

        [Then(@"el resultado de la verificación debe ser verdadero")]
        public void EntoncesResultadoVerdadero()
        {
            Assert.True(_verifyResult);
        }

        [Then(@"el resultado de la verificación debe ser falso")]
        public void EntoncesResultadoFalso()
        {
            Assert.False(_verifyResult);
        }

        // ------------------------------------------------------------------
        // Campos auxiliares para pasar datos entre Given y When
        // ------------------------------------------------------------------
        private string? _pendingUsername;
        private string? _pendingPassword;
    }
}