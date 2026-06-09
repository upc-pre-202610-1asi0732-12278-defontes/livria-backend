// US14 – Core Entity Unit Test
// Valida la lógica de la entidad Recommendation, que agrupa el conjunto de
// libros recomendados a presentar a un lector (AC1), sin dependencias externas.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using System.Collections.Generic;
using System.Linq;
using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Entities;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Commerce.Domain
{
    public class US14_RecommendationTests
    {
        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        private static Book BuildBook(string title = "El Principito") =>
            new Book(
                title:       title,
                description: "Una historia atemporal",
                author:      "Antoine de Saint-Exupéry",
                stock:       10,
                cover:       "cover.jpg",
                genre:       "fiction",
                language:    "español"
            );

        // ==================================================================
        // AC1 – Presentación de un conjunto de recomendaciones
        // ==================================================================

        [Fact]
        public void US14_AC1_CreateRecommendation_ShouldStoreUserAndBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                BuildBook("El Principito"),
                BuildBook("Cien Años de Soledad"),
            };

            // Act
            var recommendation = new Recommendation(userClientId: 42, recommendedBooks: books);

            // Assert
            Assert.Equal(42, recommendation.UserClientId);
            Assert.Equal(2,  recommendation.RecommendedBooks.Count());
        }

        [Fact]
        public void US14_AC1_RecommendedBooks_ShouldExposeEssentialFields()
        {
            // Arrange
            var books = new List<Book> { BuildBook("El Principito") };

            // Act
            var recommendation = new Recommendation(1, books);
            var first = recommendation.RecommendedBooks.First();

            // Assert — cada recomendación expone los datos esenciales para mostrarse
            Assert.False(string.IsNullOrWhiteSpace(first.Title));
            Assert.False(string.IsNullOrWhiteSpace(first.Cover));
        }

        [Fact]
        public void US14_AC1_CreateRecommendation_WhenBooksIsNull_ShouldBeEmptyNotNull()
        {
            // Arrange & Act — sin datos de preferencia aún
            var recommendation = new Recommendation(userClientId: 1, recommendedBooks: null);

            // Assert — la colección se inicializa vacía, nunca nula
            Assert.NotNull(recommendation.RecommendedBooks);
            Assert.Empty(recommendation.RecommendedBooks);
        }
    }
}
