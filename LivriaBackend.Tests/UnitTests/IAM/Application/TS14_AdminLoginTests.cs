// TS14 – Application Service Unit Test
// Valida el flujo de autenticación del administrador en
// LoginAdminCommandHandler: acceso con credenciales y pin válidos (AC1)
// y rechazo por credenciales o pin inválidos (AC2).
// Framework: xUnit + Moq | Patrón: Arrange – Act – Assert

using LivriaBackend.IAM.Application.Internal.CommandServices;
using LivriaBackend.IAM.Application.Internal.OutboundServices;
using LivriaBackend.IAM.Domain.Model.Aggregates;
using LivriaBackend.IAM.Domain.Model.Commands;
using LivriaBackend.IAM.Domain.Repositories;
using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Repositories;
using Moq;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.IAM.Application
{
    public class TS14_AdminLoginTests
    {
        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        private const string ValidUsername = "admin01";
        private const string ValidPassword = "AdminPass123";
        private const string ValidPin      = "9999";

        private readonly Mock<IIdentityRepository>  _identityRepository  = new();
        private readonly Mock<IUnitOfWork>          _unitOfWork          = new();
        private readonly Mock<ITokenService>        _tokenService        = new();
        private readonly Mock<IUserAdminRepository> _userAdminRepository = new();

        private LoginAdminCommandHandler BuildHandler() =>
            new LoginAdminCommandHandler(
                _identityRepository.Object,
                _unitOfWork.Object,
                _tokenService.Object,
                _userAdminRepository.Object);

        private static Identity BuildIdentity() =>
            new Identity(1, ValidUsername, ValidPassword);

        private static UserAdmin BuildUserAdmin(string pin = ValidPin) =>
            new UserAdmin("Admin", ValidUsername, "admin@livria.com", true, pin);

        // ==================================================================
        // AC1 – Autenticación exitosa y acceso al panel de administración
        // ==================================================================

        [Fact]
        public async Task Handle_WhenCredentialsAndPinValid_ShouldGrantAccess()
        {
            // Arrange
            var identity = BuildIdentity();
            _identityRepository.Setup(r => r.GetByUsernameAsync(ValidUsername))
                .ReturnsAsync(identity);
            _userAdminRepository.Setup(r => r.GetByIdAsync(identity.UserId))
                .ReturnsAsync(BuildUserAdmin());
            _tokenService.Setup(t => t.GenerateToken(identity, It.IsAny<IList<string>>()))
                .Returns("valid-jwt-token");

            var handler = BuildHandler();

            // Act
            var result = await handler.Handle(
                new LoginAdminCommand(ValidUsername, ValidPassword, ValidPin),
                CancellationToken.None);

            // Assert — acceso otorgado con token de sesión
            Assert.True(result.Success);
            Assert.Equal("Login successful.", result.Message);
            Assert.Equal("valid-jwt-token",   result.Token);
            Assert.Equal(ValidUsername,       result.Username);
        }

        [Fact]
        public async Task Handle_WhenLoginSuccessful_ShouldGenerateTokenWithAdminRole()
        {
            // Arrange
            var identity = BuildIdentity();
            IList<string>? capturedRoles = null;
            _identityRepository.Setup(r => r.GetByUsernameAsync(ValidUsername))
                .ReturnsAsync(identity);
            _userAdminRepository.Setup(r => r.GetByIdAsync(identity.UserId))
                .ReturnsAsync(BuildUserAdmin());
            _tokenService.Setup(t => t.GenerateToken(identity, It.IsAny<IList<string>>()))
                .Callback<Identity, IList<string>>((_, roles) => capturedRoles = roles)
                .Returns("token");

            var handler = BuildHandler();

            // Act
            await handler.Handle(
                new LoginAdminCommand(ValidUsername, ValidPassword, ValidPin),
                CancellationToken.None);

            // Assert — el token de administrador incluye el rol Admin
            Assert.NotNull(capturedRoles);
            Assert.Contains("Admin", capturedRoles!);
        }

        // ==================================================================
        // AC2 – Rechazo por credenciales no válidas
        // ==================================================================

        [Fact]
        public async Task Handle_WhenUsernameNotFound_ShouldDenyAccess()
        {
            // Arrange — no existe ningún registro con ese usuario
            _identityRepository.Setup(r => r.GetByUsernameAsync("ghost"))
                .ReturnsAsync((Identity?)null);

            var handler = BuildHandler();

            // Act
            var result = await handler.Handle(
                new LoginAdminCommand("ghost", ValidPassword, ValidPin),
                CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.Message);
        }

        [Fact]
        public async Task Handle_WhenPasswordIsWrong_ShouldDenyAccess()
        {
            // Arrange
            _identityRepository.Setup(r => r.GetByUsernameAsync(ValidUsername))
                .ReturnsAsync(BuildIdentity());

            var handler = BuildHandler();

            // Act
            var result = await handler.Handle(
                new LoginAdminCommand(ValidUsername, "WrongPass!", ValidPin),
                CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.Message);
        }

        [Fact]
        public async Task Handle_WhenUserIsNotAdmin_ShouldDenyAccess()
        {
            // Arrange — credenciales válidas pero sin registro de administrador
            var identity = BuildIdentity();
            _identityRepository.Setup(r => r.GetByUsernameAsync(ValidUsername))
                .ReturnsAsync(identity);
            _userAdminRepository.Setup(r => r.GetByIdAsync(identity.UserId))
                .ReturnsAsync((UserAdmin?)null);

            var handler = BuildHandler();

            // Act
            var result = await handler.Handle(
                new LoginAdminCommand(ValidUsername, ValidPassword, ValidPin),
                CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password for admin login.", result.Message);
        }

        [Fact]
        public async Task Handle_WhenSecurityPinIsWrong_ShouldDenyAccess()
        {
            // Arrange
            var identity = BuildIdentity();
            _identityRepository.Setup(r => r.GetByUsernameAsync(ValidUsername))
                .ReturnsAsync(identity);
            _userAdminRepository.Setup(r => r.GetByIdAsync(identity.UserId))
                .ReturnsAsync(BuildUserAdmin(pin: ValidPin));

            var handler = BuildHandler();

            // Act
            var result = await handler.Handle(
                new LoginAdminCommand(ValidUsername, ValidPassword, "0000"),
                CancellationToken.None);

            // Assert — el pin de seguridad incorrecto bloquea el acceso
            Assert.False(result.Success);
            Assert.Equal("Invalid security pin.", result.Message);
        }

        [Fact]
        public async Task Handle_WhenAccessDenied_ShouldNotGenerateToken()
        {
            // Arrange
            _identityRepository.Setup(r => r.GetByUsernameAsync(ValidUsername))
                .ReturnsAsync(BuildIdentity());

            var handler = BuildHandler();

            // Act
            var result = await handler.Handle(
                new LoginAdminCommand(ValidUsername, "WrongPass!", ValidPin),
                CancellationToken.None);

            // Assert — sin acceso no se emite ningún token de sesión
            Assert.Null(result.Token);
            _tokenService.Verify(
                t => t.GenerateToken(It.IsAny<Identity>(), It.IsAny<IList<string>>()),
                Times.Never);
        }
    }
}
