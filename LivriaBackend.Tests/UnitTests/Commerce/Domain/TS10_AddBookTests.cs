// TS10 – Core Entity Unit Test
// Valida la lógica del agregado Book para el registro de un nuevo libro:
// - Construcción con datos válidos (AC1/AC2)
// - Generación automática de precios por género
// - Validaciones de idioma, género y stock
// Framework: xUnit | Patrón: Arrange – Act – Assert

using LivriaBackend.commerce.Domain.Model.Aggregates;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Commerce.Domain
{
    public class TS10_AddBookTests
    {
        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        private static Book BuildBook(
            string title    = "Clean Code",
            string author   = "Robert C. Martin",
            string genre    = "non_fiction",
            string language = "english",
            int    stock    = 10) =>
            new Book(
                title:       title,
                description: "A handbook of agile software craftsmanship",
                author:      author,
                stock:       stock,
                cover:       "https://livria.com/covers/clean_code.jpg",
                genre:       genre,
                language:    language
            );

        // ==================================================================
        // AC1 – Interfaz presenta campos requeridos (validación de dominio)
        // ==================================================================

        [Fact]
        public void TS10_AC1_Book_WhenAllFieldsValid_ShouldCreateSuccessfully()
        {
            // Arrange & Act
            var book = BuildBook();

            // Assert — todos los campos esenciales están presentes
            Assert.Equal("Clean Code",       book.Title);
            Assert.Equal("Robert C. Martin", book.Author);
            Assert.Equal("non_fiction",      book.Genre);
            Assert.Equal("english",          book.Language);
            Assert.Equal(10,                 book.Stock);
            Assert.False(string.IsNullOrWhiteSpace(book.Cover));
        }

        [Fact]
        public void TS10_AC1_Book_WhenCreated_ShouldBeActiveByDefault()
        {
            // Arrange & Act
            var book = BuildBook();

            // Assert — el libro recién registrado está activo en el inventario
            Assert.True(book.IsActive,
                "AC1: un libro recién registrado debe aparecer en el listado del inventario");
        }

        [Fact]
        public void TS10_AC1_Book_WhenCreated_ShouldGeneratePurchasePriceAutomatically()
        {
            // Arrange & Act
            var book = BuildBook(genre: "non_fiction");

            // Assert — precio de compra generado dentro del rango de non_fiction (20-30)
            Assert.True(book.PurchasePrice >= 20m && book.PurchasePrice <= 30m,
                $"PurchasePrice={book.PurchasePrice} debe estar entre 20 y 30 para non_fiction");
        }

        [Fact]
        public void TS10_AC1_Book_WhenCreated_SalePriceShouldBe165PercentOfPurchasePrice()
        {
            // Arrange & Act
            var book = BuildBook();

            // Assert — SalePrice = PurchasePrice * 1.65
            var expectedSalePrice = book.PurchasePrice * 1.65m;
            Assert.Equal(expectedSalePrice, book.SalePrice);
        }

        // ==================================================================
        // AC2 – Validación de integridad antes del registro
        // ==================================================================

        [Theory]
        [InlineData("literature")]
        [InlineData("non_fiction")]
        [InlineData("fiction")]
        [InlineData("mangas_comics")]
        [InlineData("juvenile")]
        [InlineData("children")]
        [InlineData("ebooks_audiobooks")]
        public void TS10_AC2_Book_WhenGenreIsValid_ShouldCreateSuccessfully(string genre)
        {
            // Arrange & Act
            var book = BuildBook(genre: genre);

            // Assert
            Assert.Equal(genre, book.Genre);
        }

        [Fact]
        public void TS10_AC2_Book_WhenGenreIsInvalid_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                BuildBook(genre: "drama"));

            Assert.Contains("género", ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("english")]
        [InlineData("español")]
        public void TS10_AC2_Book_WhenLanguageIsValid_ShouldCreateSuccessfully(string language)
        {
            // Arrange & Act
            var book = BuildBook(language: language);

            // Assert
            Assert.Equal(language, book.Language);
        }

        [Fact]
        public void TS10_AC2_Book_WhenLanguageIsInvalid_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                BuildBook(language: "french"));

            Assert.Contains("idioma", ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TS10_AC2_Book_WhenStockIsNegative_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BuildBook(stock: -1));
        }

        [Fact]
        public void TS10_AC2_Book_WhenStockIsZero_ShouldCreateSuccessfully()
        {
            // Un libro puede registrarse con stock 0 (pendiente de recepción)
            // Arrange & Act
            var book = BuildBook(stock: 0);

            // Assert
            Assert.Equal(0, book.Stock);
        }

        // ==================================================================
        // AddStock – AC2: el listado se actualiza al agregar unidades
        // ==================================================================

        [Fact]
        public void TS10_AC2_Book_AddStock_ShouldIncreaseStockCorrectly()
        {
            // Arrange
            var book = BuildBook(stock: 10);

            // Act
            book.AddStock(5);

            // Assert — el stock se refleja correctamente en el inventario
            Assert.Equal(15, book.Stock);
        }

        [Fact]
        public void TS10_AC2_Book_AddStock_WhenNegativeQuantity_ShouldThrow()
        {
            // Arrange
            var book = BuildBook(stock: 10);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                book.AddStock(-1));
        }

        // ==================================================================
        // Deactivate / Reactivate – gestión del inventario
        // ==================================================================

        [Fact]
        public void TS10_AC2_Book_Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var book = BuildBook();

            // Act
            book.Deactivate();

            // Assert — libro eliminado no aparece en el listado
            Assert.False(book.IsActive);
        }

        [Fact]
        public void TS10_AC2_Book_Reactivate_ShouldSetIsActiveToTrue()
        {
            // Arrange
            var book = BuildBook();
            book.Deactivate();

            // Act
            book.Reactivate();

            // Assert — libro reactivado vuelve a aparecer en el listado
            Assert.True(book.IsActive);
        }
    }
}
