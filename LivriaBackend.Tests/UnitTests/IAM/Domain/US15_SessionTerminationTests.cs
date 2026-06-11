// US15 – Core Entity Unit Test
// Valida la terminación segura de la sesión (cierre de sesión) a través del
// TokenService: una sesión activa otorga acceso, y tras eliminar o invalidar
// el token de autenticación, el sistema deniega el acceso.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using System.Security.Claims;
using System.Text;
using LivriaBackend.IAM.Domain.Model.Aggregates;
using LivriaBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using LivriaBackend.IAM.Infrastructure.Tokens.JWT.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.IAM.Domain
{
    public class US15_SessionTerminationTests
    {
        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        private const string Secret =
            "UnitTestSecretKeyForUS15SessionTermination_MustBeLongEnough!";

        private static TokenService BuildTokenService(string secret = Secret) =>
            new TokenService(Options.Create(new TokenSettings { Secret = secret }));

        private static Identity BuildIdentity(
            int userId = 1,
            string username = "lector01") =>
            new Identity(userId, username, "SecurePass123");

        // Genera un token con fecha de expiración arbitraria, firmado con la
        // misma clave que usa el TokenService (simula una sesión antigua).
        private static string GenerateTokenWithExpiration(
            Identity user, DateTime expires, string secret = Secret)
        {
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        }

        // ==================================================================
        // AC1 – Sesión activa: el token vigente otorga acceso
        // ==================================================================

        [Fact]
        public async Task ValidateToken_WhenSessionActive_ShouldReturnUserId()
        {
            // Arrange
            var service = BuildTokenService();
            var identity = BuildIdentity(userId: 1);
            var token = service.GenerateToken(identity, new List<string> { "Client" });

            // Act
            var result = await service.ValidateToken(token);

            // Assert — la sesión activa identifica correctamente al lector
            Assert.Equal(identity.Id, result);
        }

        // ==================================================================
        // AC1 – Eliminación de los datos de autenticación del dispositivo:
        // sin token no hay acceso
        // ==================================================================

        [Fact]
        public async Task ValidateToken_WhenTokenRemoved_ShouldDenyAccess()
        {
            // Arrange — tras el cierre de sesión el dispositivo ya no posee token
            var service = BuildTokenService();

            // Act
            var resultEmpty = await service.ValidateToken(string.Empty);
            var resultNull = await service.ValidateToken(null);

            // Assert
            Assert.Null(resultEmpty);
            Assert.Null(resultNull);
        }

        // ==================================================================
        // AC1 – Invalidación segura: un token expirado no otorga acceso
        // ==================================================================

        [Fact]
        public async Task ValidateToken_WhenTokenExpired_ShouldDenyAccess()
        {
            // Arrange — sesión cuyo tiempo de vida ya terminó
            var service = BuildTokenService();
            var identity = BuildIdentity();
            var expiredToken = GenerateTokenWithExpiration(
                identity, DateTime.UtcNow.AddMinutes(-5));

            // Act
            var result = await service.ValidateToken(expiredToken);

            // Assert
            Assert.Null(result);
        }

        // ==================================================================
        // AC1 – Protección de la información personal:
        // tokens manipulados o ajenos son rechazados
        // ==================================================================

        [Fact]
        public async Task ValidateToken_WhenTokenTampered_ShouldDenyAccess()
        {
            // Arrange — un token alterado no debe reactivar la sesión
            var service = BuildTokenService();
            var identity = BuildIdentity();
            var token = service.GenerateToken(identity, new List<string> { "Client" });
            var tamperedToken = token.Substring(0, token.Length - 4) + "abcd";

            // Act
            var result = await service.ValidateToken(tamperedToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateToken_WhenTokenSignedWithDifferentKey_ShouldDenyAccess()
        {
            // Arrange — token emitido con otra clave (firma no confiable)
            var service = BuildTokenService();
            var identity = BuildIdentity();
            var foreignToken = GenerateTokenWithExpiration(
                identity,
                DateTime.UtcNow.AddDays(1),
                secret: "AnotherSecretKeyThatDoesNotBelongToLivria_Long!");

            // Act
            var result = await service.ValidateToken(foreignToken);

            // Assert
            Assert.Null(result);
        }
    }
}
