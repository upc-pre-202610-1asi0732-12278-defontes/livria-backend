// US19 – Core Entity Unit Test
// Valida la lógica de búsqueda por título y por autor
// sobre el agregado Book sin dependencias externas.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using LivriaBackend.commerce.Domain.Model.Aggregates;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Commerce.Domain
{
    public class US19_BookSearchTests
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

        // Helper: simula el filtro que aplica SearchPage
        private static List<Book> FilterBooks(
            IEnumerable<Book> books, string query) =>
            books.Where(b =>
                b.Title.Contains(query,  StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase)
            ).ToList();

        // ==================================================================
        // AC1 – Búsqueda por título
        // ==================================================================

        [Fact]
        public void US19_AC1_SearchByTitle_WhenExactMatch_ShouldReturnBook()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(title: "El Principito"),
                BuildBook(title: "Cien Años de Soledad"),
                BuildBook(title: "Don Quijote"),
            };

            // Act
            var results = FilterBooks(catalog, "El Principito");

            // Assert
            Assert.Single(results);
            Assert.Equal("El Principito", results[0].Title);
        }

        [Fact]
        public void US19_AC1_SearchByTitle_WhenPartialMatch_ShouldReturnMatchingBooks()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(title: "El Principito"),
                BuildBook(title: "El Alquimista"),
                BuildBook(title: "Cien Años de Soledad"),
            };

            // Act — búsqueda parcial por "el"
            var results = FilterBooks(catalog, "el");

            // Assert — devuelve los dos libros que contienen "el"
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void US19_AC1_SearchByTitle_WhenCaseInsensitive_ShouldReturnBook()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(title: "El Principito"),
            };

            // Act — búsqueda en mayúsculas
            var results = FilterBooks(catalog, "EL PRINCIPITO");

            // Assert
            Assert.Single(results);
        }

        [Fact]
        public void US19_AC1_SearchByTitle_WhenNoMatch_ShouldReturnEmptyList()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(title: "El Principito"),
                BuildBook(title: "Don Quijote"),
            };

            // Act
            var results = FilterBooks(catalog, "Harry Potter");

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void US19_AC1_SearchResult_ShouldContainEssentialFields()
        {
            // Arrange
            var book = BuildBook(
                title:  "El Principito",
                author: "Antoine de Saint-Exupéry"
            );

            // Assert — los campos esenciales de presentación (AC1) no son vacíos
            Assert.False(string.IsNullOrWhiteSpace(book.Title));
            Assert.False(string.IsNullOrWhiteSpace(book.Author));
            Assert.False(string.IsNullOrWhiteSpace(book.Cover));
        }

        // ==================================================================
        // AC2 – Búsqueda por autor
        // ==================================================================

        [Fact]
        public void US19_AC2_SearchByAuthor_WhenExactMatch_ShouldReturnAllBooksOfAuthor()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(title: "Cien Años de Soledad",
                    author: "Gabriel García Márquez"),
                BuildBook(title: "El Amor en los Tiempos del Cólera",
                    author: "Gabriel García Márquez"),
                BuildBook(title: "El Principito",
                    author: "Antoine de Saint-Exupéry"),
            };

            // Act
            var results = FilterBooks(catalog, "Gabriel García Márquez");

            // Assert — devuelve TODOS los libros del autor
            Assert.Equal(2, results.Count);
            Assert.All(results, b =>
                Assert.Equal("Gabriel García Márquez", b.Author));
        }

        [Fact]
        public void US19_AC2_SearchByAuthor_WhenPartialName_ShouldReturnMatchingBooks()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(title: "Cien Años de Soledad",
                    author: "Gabriel García Márquez"),
                BuildBook(title: "El Coronel no tiene quien le escriba",
                    author: "Gabriel García Márquez"),
                BuildBook(title: "Don Quijote",
                    author: "Miguel de Cervantes"),
            };

            // Act — búsqueda parcial por apellido
            var results = FilterBooks(catalog, "García");

            // Assert
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void US19_AC2_SearchByAuthor_WhenCaseInsensitive_ShouldReturnBooks()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(author: "Gabriel García Márquez"),
            };

            // Act
            var results = FilterBooks(catalog, "gabriel garcía márquez");

            // Assert
            Assert.Single(results);
        }

        [Fact]
        public void US19_AC2_SearchByAuthor_WhenNoMatch_ShouldReturnEmptyList()
        {
            // Arrange
            var catalog = new List<Book>
            {
                BuildBook(author: "Gabriel García Márquez"),
                BuildBook(author: "Antoine de Saint-Exupéry"),
            };

            // Act
            var results = FilterBooks(catalog, "J.K. Rowling");

            // Assert
            Assert.Empty(results);
        }

        // ==================================================================
        // Book — validaciones de construcción
        // ==================================================================

        [Fact]
        public void US19_Book_WhenInvalidLanguage_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                BuildBook(language: "french"));
        }

        [Fact]
        public void US19_Book_WhenInvalidGenre_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new Book("Título", "Desc", "Autor", 5,
                    "cover.jpg", "drama", "english"));
        }
    }
}