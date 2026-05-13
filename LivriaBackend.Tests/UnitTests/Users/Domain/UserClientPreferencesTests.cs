// US12 – Core Entity Unit Test
// Proyecto: LivriaBackend.Tests
// Framework: xUnit  |  Patrón: Arrange – Act – Assert
// Valida AddFavoriteBook y AddExclusionBook del agregado UserClient
// sin ninguna dependencia externa (sin DB, sin HTTP, sin repositorios).

using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Aggregates;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Users.Domain
{
    public class UserClientPreferencesTests
    {
        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        private static UserClient BuildClient() =>
            new UserClient(
                display:      "Test Reader",
                username:     "testreader",
                email:        "reader@livria.com",
                icon:         "icon.png",
                phrase:       "I love books",
                subscription: "freeplan"
            );

        private static Book BuildBook(string title = "Clean Code") =>
            new Book(
                title:       title,
                description: "A handbook of agile software craftsmanship",
                author:      "Robert C. Martin",
                stock:       10,
                cover:       "cover.jpg",
                genre:       "non_fiction",
                language:    "english"
            );

        // ------------------------------------------------------------------
        // AC2 – Registro de preferencia POSITIVA
        // ------------------------------------------------------------------

        [Fact]
        public void AddFavoriteBook_WhenBookIsValid_ShouldAppearInFavorites()
        {
            // Arrange
            var client = BuildClient();
            var book   = BuildBook();

            // Act
            client.AddFavoriteBook(book);

            // Assert
            Assert.Contains(book, client.FavoriteBooks);
        }

        [Fact]
        public void AddFavoriteBook_WhenBookIsAlreadyFavorite_ShouldNotDuplicate()
        {
            // Arrange
            var client = BuildClient();
            var book   = BuildBook();

            // Act
            client.AddFavoriteBook(book);
            client.AddFavoriteBook(book); // segunda vez

            // Assert
            Assert.Single(client.FavoriteBooks);
        }

        [Fact]
        public void AddFavoriteBook_WhenBookWasPreviouslyExcluded_ShouldRemoveFromExclusions()
        {
            // Arrange
            var client = BuildClient();
            var book   = BuildBook();
            client.AddExclusionBook(book); // primero excluido

            // Act
            client.AddFavoriteBook(book); // ahora marcado como favorito

            // Assert — ya no debe estar excluido
            Assert.DoesNotContain(book, client.ExclusionBooks);
            Assert.Contains(book, client.FavoriteBooks);
        }

        // ------------------------------------------------------------------
        // AC3 – Registro de preferencia NEGATIVA
        // ------------------------------------------------------------------

        [Fact]
        public void AddExclusionBook_WhenBookIsValid_ShouldAppearInExclusions()
        {
            // Arrange
            var client = BuildClient();
            var book   = BuildBook("Book to hide");

            // Act
            client.AddExclusionBook(book);

            // Assert
            Assert.Contains(book, client.ExclusionBooks);
        }

        [Fact]
        public void AddExclusionBook_WhenBookWasFavorite_ShouldRemoveFromFavorites()
        {
            // Arrange
            var client = BuildClient();
            var book   = BuildBook("Book to hide");
            client.AddFavoriteBook(book); // primero favorito

            // Act
            client.AddExclusionBook(book); // ahora rechazado

            // Assert
            Assert.DoesNotContain(book, client.FavoriteBooks);
            Assert.Contains(book, client.ExclusionBooks);
        }

        [Fact]
        public void AddExclusionBook_WhenBookIsAlreadyExcluded_ShouldNotDuplicate()
        {
            // Arrange
            var client = BuildClient();
            var book   = BuildBook("Book to hide");

            // Act
            client.AddExclusionBook(book);
            client.AddExclusionBook(book); // segunda vez

            // Assert
            Assert.Single(client.ExclusionBooks);
        }
    }
}