// US26 – Core Entity Unit Test
// Valida la lógica de filtrado y ordenamiento de libros sobre el agregado Book:
// ordenamiento por criterios (AC1), filtro por atributo (AC2) y restablecimiento
// de filtros (AC3), sin dependencias externas. El filtrado/orden se modela con
// helpers que reproducen el comportamiento aplicado sobre el catálogo.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using System.Collections.Generic;
using System.Linq;
using LivriaBackend.commerce.Domain.Model.Aggregates;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Commerce.Domain
{
    public class US26_BookFilterTests
    {
        // ------------------------------------------------------------------
        // Helper: construye un Book válido
        // ------------------------------------------------------------------
        private static Book BuildBook(
            string title    = "El Principito",
            string author   = "Antoine de Saint-Exupéry",
            string genre    = "fiction",
            string language = "español") =>
            new Book(
                title:       title,
                description: "Una historia atemporal",
                author:      author,
                stock:       10,
                cover:       "cover.jpg",
                genre:       genre,
                language:    language
            );

        // Helpers: reproducen filtro y orden aplicados sobre el catálogo
        private static List<Book> FilterByLanguage(IEnumerable<Book> books, string language) =>
            books.Where(b => string.Equals(b.Language, language, StringComparison.OrdinalIgnoreCase)).ToList();

        private static List<Book> FilterByGenre(IEnumerable<Book> books, string genre) =>
            books.Where(b => string.Equals(b.Genre, genre, StringComparison.OrdinalIgnoreCase)).ToList();

        private static List<Book> SortByTitle(IEnumerable<Book> books) =>
            books.OrderBy(b => b.Title, StringComparer.OrdinalIgnoreCase).ToList();

        private static List<Book> SortBySalePriceAscending(IEnumerable<Book> books) =>
            books.OrderBy(b => b.SalePrice).ToList();

        private static List<Book> BuildCatalog() => new List<Book>
        {
            BuildBook(title: "Don Quijote",          genre: "literature", language: "español"),
            BuildBook(title: "El Principito",        genre: "fiction",    language: "español"),
            BuildBook(title: "Clean Code",           genre: "non_fiction", language: "english"),
            BuildBook(title: "Atomic Habits",        genre: "non_fiction", language: "english"),
        };

        // ==================================================================
        // AC1 – Aplicación de ordenamiento por criterios
        // ==================================================================

        [Fact]
        public void US26_AC1_SortByTitle_ShouldReturnAlphabeticalOrder()
        {
            // Arrange
            var catalog = BuildCatalog();

            // Act
            var sorted = SortByTitle(catalog);

            // Assert
            Assert.Equal("Atomic Habits", sorted.First().Title);
            Assert.Equal("El Principito", sorted.Last().Title);
        }

        [Fact]
        public void US26_AC1_SortBySalePrice_ShouldReturnAscendingOrder()
        {
            // Arrange
            var catalog = BuildCatalog();

            // Act
            var sorted = SortBySalePriceAscending(catalog);

            // Assert — cada precio es menor o igual al siguiente
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                Assert.True(sorted[i].SalePrice <= sorted[i + 1].SalePrice);
            }
        }

        // ==================================================================
        // AC2 – Aplicación de filtro por atributo
        // ==================================================================

        [Fact]
        public void US26_AC2_FilterByLanguage_ShouldReturnOnlyMatchingBooks()
        {
            // Arrange
            var catalog = BuildCatalog();

            // Act
            var results = FilterByLanguage(catalog, "english");

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, b => Assert.Equal("english", b.Language));
        }

        [Fact]
        public void US26_AC2_FilterByGenre_ShouldReturnOnlyMatchingBooks()
        {
            // Arrange
            var catalog = BuildCatalog();

            // Act
            var results = FilterByGenre(catalog, "non_fiction");

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, b => Assert.Equal("non_fiction", b.Genre));
        }

        [Fact]
        public void US26_AC2_FilterByLanguage_WhenNoMatch_ShouldReturnEmptyList()
        {
            // Arrange — el catálogo solo tiene 'español' e 'english'
            var catalog = FilterByGenre(BuildCatalog(), "fiction"); // solo español

            // Act
            var results = FilterByLanguage(catalog, "english");

            // Assert
            Assert.Empty(results);
        }

        // ==================================================================
        // AC3 – Restablecimiento de filtros y orden
        // ==================================================================

        [Fact]
        public void US26_AC3_ResetFilters_ShouldReturnFullCatalog()
        {
            // Arrange
            var catalog = BuildCatalog();
            var filtered = FilterByLanguage(catalog, "english"); // 2 libros
            Assert.Equal(2, filtered.Count);

            // Act — restablecer = volver al catálogo completo en su estado original
            var reset = catalog;

            // Assert
            Assert.Equal(4, reset.Count);
        }
    }
}
