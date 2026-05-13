// US13 – Core Entity Unit Test
// Valida la lógica de registro y autenticación del agregado Identity
// y el Value Object PasswordHash, sin dependencias externas.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using LivriaBackend.IAM.Domain.Model.Aggregates;
using LivriaBackend.IAM.Domain.Model.ValueObjects;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.IAM.Domain
{
    public class US13_IdentityAuthTests
    {
        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        private static Identity BuildIdentity(
            string username = "lector01",
            string password = "SecurePass123") =>
            new Identity(
                userid:         1,
                username:       username,
                hashedPassword: password
            );

        // ==================================================================
        // AC1 – Creación y almacenamiento seguro de credenciales
        // ==================================================================

        [Fact]
        public void Constructor_WhenValidCredentials_ShouldStoreHashedPassword()
        {
            // Arrange
            const string plainPassword = "SecurePass123";

            // Act
            var identity = BuildIdentity(password: plainPassword);

            // Assert — la contraseña NO se guarda en texto plano
            Assert.NotNull(identity.HashedPassword);
            Assert.NotEqual(plainPassword, identity.HashedPassword.HashedValue);
        }

        [Fact]
        public void Constructor_WhenValidCredentials_ShouldSetUsername()
        {
            // Arrange & Act
            var identity = BuildIdentity(username: "lector01");

            // Assert
            Assert.Equal("lector01", identity.UserName);
        }

        [Fact]
        public void Constructor_WhenUsernameTooShort_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Identity(1, "ab", "ValidPass123"));

            Assert.Contains("between 3 and 50", ex.Message);
        }

        [Fact]
        public void Constructor_WhenPasswordTooShort_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Identity(1, "validuser", "ab"));

            Assert.Contains("between 3 and 100", ex.Message);
        }

        // ==================================================================
        // AC2 – Autenticación exitosa con credenciales correctas
        // ==================================================================

        [Fact]
        public void VerifyPassword_WhenCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            const string plainPassword = "SecurePass123";
            var identity = BuildIdentity(password: plainPassword);

            // Act
            var result = identity.VerifyPassword(plainPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WhenWrongPassword_ShouldReturnFalse()
        {
            // Arrange
            var identity = BuildIdentity(password: "SecurePass123");

            // Act
            var result = identity.VerifyPassword("WrongPassword!");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_WhenEmptyPassword_ShouldReturnFalse()
        {
            // Arrange
            var identity = BuildIdentity(password: "SecurePass123");

            // Act
            var result = identity.VerifyPassword(string.Empty);

            // Assert
            Assert.False(result);
        }

        // ==================================================================
        // PasswordHash Value Object
        // ==================================================================

        [Fact]
        public void PasswordHash_TwoInstancesWithSameInput_ShouldHaveDifferentHashes()
        {
            // BCrypt usa salt aleatorio — dos hashes del mismo texto son distintos
            // pero ambos deben verificar correctamente
            var hash1 = new PasswordHash("SamePassword");
            var hash2 = new PasswordHash("SamePassword");

            Assert.NotEqual(hash1.HashedValue, hash2.HashedValue);
            Assert.True(hash1.Matches("SamePassword"));
            Assert.True(hash2.Matches("SamePassword"));
        }

        [Fact]
        public void UpdatePassword_WhenCurrentPasswordCorrect_ShouldUpdateSuccessfully()
        {
            // Arrange
            var identity = BuildIdentity(password: "OldPass123");

            // Act
            identity.UpdatePassword("OldPass123", "NewPass456");

            // Assert — nueva contraseña verifica correctamente
            Assert.True(identity.VerifyPassword("NewPass456"));
            Assert.False(identity.VerifyPassword("OldPass123"));
        }

        [Fact]
        public void UpdatePassword_WhenCurrentPasswordWrong_ShouldThrowArgumentException()
        {
            // Arrange
            var identity = BuildIdentity(password: "OldPass123");

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                identity.UpdatePassword("WrongPass!", "NewPass456"));
        }
    }
}