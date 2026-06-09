// US21 – Core Entity Unit Test
// Valida la lógica de valoración y reseñas del libro sobre la entidad Review:
// registro de puntuación (AC1), publicación de reseña (AC2) y validaciones
// de rango de estrellas, sin dependencias externas.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using LivriaBackend.commerce.Domain.Model.Entities;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Commerce.Domain
{
    public class US21_ReviewRatingTests
    {
        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        private static Review BuildReview(
            int    bookId       = 1,
            int    userClientId = 42,
            string content      = "Una lectura imprescindible",
            int    stars        = 5,
            string username     = "lector01") =>
            new Review(bookId, userClientId, content, stars, username);

        // ==================================================================
        // AC1 – Registro de la puntuación de un libro
        // ==================================================================

        [Fact]
        public void US21_AC1_CreateReview_ShouldStoreStarsAndBookLink()
        {
            // Arrange & Act
            var review = BuildReview(bookId: 7, stars: 4);

            // Assert — la valoración queda vinculada al libro
            Assert.Equal(7, review.BookId);
            Assert.Equal(4, review.Stars);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void US21_AC1_UpdateStars_WhenWithinRange_ShouldUpdate(int stars)
        {
            // Arrange
            var review = BuildReview();

            // Act
            review.Update("Reseña editada", stars);

            // Assert — se aceptan los límites válidos (1 a 5)
            Assert.Equal(stars, review.Stars);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public void US21_AC1_UpdateStars_WhenOutOfRange_ShouldThrowArgumentOutOfRangeException(int stars)
        {
            // Arrange
            var review = BuildReview();

            // Act & Assert — la calificación debe estar entre 1 y 5
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                review.Update("Contenido válido", stars));
        }

        // ==================================================================
        // AC2 – Envío y publicación de una reseña
        // ==================================================================

        [Fact]
        public void US21_AC2_CreateReview_ShouldLinkContentToBookAndUser()
        {
            // Arrange & Act
            var review = BuildReview(
                bookId:       3,
                userClientId: 88,
                content:      "Me encantó el final",
                username:     "ana_reads");

            // Assert — la reseña queda vinculada al libro y al lector
            Assert.Equal(3,                  review.BookId);
            Assert.Equal(88,                 review.UserClientId);
            Assert.Equal("Me encantó el final", review.Content);
            Assert.Equal("ana_reads",        review.Username);
        }

        [Fact]
        public void US21_AC2_Update_ShouldChangeContent()
        {
            // Arrange
            var review = BuildReview(content: "Contenido original");

            // Act
            review.Update("Contenido actualizado", 4);

            // Assert
            Assert.Equal("Contenido actualizado", review.Content);
        }

        [Fact]
        public void US21_AC2_Update_WhenContentIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var review = BuildReview();

            // Act & Assert — el contenido textual no puede ser vacío
            Assert.Throws<ArgumentException>(() => review.Update("   ", 4));
        }

        // ==================================================================
        // Anonimización (eliminación de cuenta)
        // ==================================================================

        [Fact]
        public void US21_Anonymize_ShouldReassignReviewToDeletedUser()
        {
            // Arrange
            var review = BuildReview(userClientId: 88, username: "ana_reads");

            // Act
            review.Anonymize();

            // Assert
            Assert.Equal(UserConstants.DeletedUserId,   review.UserClientId);
            Assert.Equal(UserConstants.DeletedUsername, review.Username);
        }
    }
}
