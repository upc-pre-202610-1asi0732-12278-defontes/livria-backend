// US16 – Core Entity Unit Test
// Valida la lógica del agregado Post para creación con imagen (AC1)
// y con solo contenido textual (AC2), sin dependencias externas.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using LivriaBackend.communities.Domain.Model.Aggregates;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Communities.Domain
{
    public class US16_PostCreationTests
    {
        // ------------------------------------------------------------------
        // Helper: construye un Post base válido
        // ------------------------------------------------------------------
        private static Post BuildPost(
            int    communityId = 1,
            int    userId      = 42,
            string username    = "lector01",
            string content     = "Mi reseña del libro",
            string img         = "") =>
            new Post(communityId, userId, username, content, img);

        // ==================================================================
        // AC1 – Creación de publicación CON imagen
        // ==================================================================

        [Fact]
        public void US16_AC1_CreatePost_WithImage_ShouldStoreImgUrl()
        {
            // Arrange
            const string imageUrl = "https://livria.com/uploads/post_img.jpg";

            // Act
            var post = BuildPost(img: imageUrl);

            // Assert — la imagen se almacena correctamente
            Assert.Equal(imageUrl, post.Img);
        }

        [Fact]
        public void US16_AC1_CreatePost_WithImage_ShouldSetCommunityAndUser()
        {
            // Arrange
            const int communityId = 5;
            const int userId      = 42;

            // Act
            var post = BuildPost(communityId: communityId, userId: userId,
                img: "https://livria.com/uploads/img.jpg");

            // Assert — el post queda vinculado a la comunidad y al usuario
            Assert.Equal(communityId, post.CommunityId);
            Assert.Equal(userId,      post.UserId);
        }

        [Fact]
        public void US16_AC1_CreatePost_WithImage_ShouldSetCreatedAtToUtcNow()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var post = BuildPost(img: "https://livria.com/uploads/img.jpg");

            // Assert — la fecha de creación es UTC y está dentro del rango esperado
            var after = DateTime.UtcNow;
            Assert.True(post.CreatedAt >= before && post.CreatedAt <= after,
                $"CreatedAt={post.CreatedAt} debe estar entre {before} y {after}");
        }

        // ==================================================================
        // AC2 – Creación de publicación con SOLO contenido textual
        // ==================================================================

        [Fact]
        public void US16_AC2_CreatePost_TextOnly_ShouldStoreContent()
        {
            // Arrange
            const string textContent = "Acabo de terminar Cien Años de Soledad. ¡Increíble!";

            // Act
            var post = BuildPost(content: textContent, img: "");

            // Assert — el contenido textual se registra correctamente
            Assert.Equal(textContent, post.Content);
        }

        [Fact]
        public void US16_AC2_CreatePost_TextOnly_ShouldHaveEmptyImg()
        {
            // Arrange & Act
            var post = BuildPost(content: "Solo texto, sin imagen", img: "");

            // Assert — no hay imagen asociada al post
            Assert.Equal(string.Empty, post.Img);
        }

        [Fact]
        public void US16_AC2_CreatePost_TextOnly_ShouldSetUsername()
        {
            // Arrange
            const string expectedUsername = "lector01";

            // Act
            var post = BuildPost(username: expectedUsername, img: "");

            // Assert — el username del autor se registra correctamente
            Assert.Equal(expectedUsername, post.Username);
        }

        // ==================================================================
        // Update — el post puede editarse conservando o cambiando imagen
        // ==================================================================

        [Fact]
        public void US16_Update_ShouldChangeContentAndImg()
        {
            // Arrange
            var post = BuildPost(content: "Contenido original", img: "");

            // Act
            post.Update("Contenido editado", "https://livria.com/uploads/nueva.jpg");

            // Assert
            Assert.Equal("Contenido editado",                        post.Content);
            Assert.Equal("https://livria.com/uploads/nueva.jpg",    post.Img);
        }

        [Fact]
        public void US16_Update_WhenRemovingImage_ShouldSetEmptyImg()
        {
            // Arrange
            var post = BuildPost(img: "https://livria.com/uploads/img.jpg");

            // Act — el usuario elimina la imagen al editar
            post.Update("Solo texto ahora", "");

            // Assert
            Assert.Equal(string.Empty, post.Img);
        }
    }
}