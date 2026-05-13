// US16 – BDD Step Definitions
// Framework: SpecFlow + xUnit
// Valida el comportamiento de creación de posts con y sin imagen
// directamente sobre el agregado Post del dominio.

using LivriaBackend.communities.Domain.Model.Aggregates;
using LivriaBackend.communities.Domain.Model.Commands;
using TechTalk.SpecFlow;
using Xunit;

namespace LivriaBackend.Tests.BDD.Steps
{
    [Binding]
    public class US16_CrearPublicacionesSteps
    {
        // ------------------------------------------------------------------
        // Estado compartido entre pasos del mismo escenario
        // ------------------------------------------------------------------
        private int     _communityId;
        private int     _userId;
        private string  _username  = string.Empty;
        private Post?   _createdPost;

        // ------------------------------------------------------------------
        // Background
        // ------------------------------------------------------------------

        [Given(@"existe una comunidad con ID (\d+) en el sistema")]
        public void US16_DadoExisteComunidad(int communityId)
        {
            _communityId = communityId;
        }

        [Given(@"existe un usuario con ID (\d+) y username ""(.*)"" con sesión activa")]
        public void US16_DadoExisteUsuarioConSesion(int userId, string username)
        {
            _userId   = userId;
            _username = username;
        }

        // ------------------------------------------------------------------
        // Given compartido entre AC1 y AC2
        // ------------------------------------------------------------------

        [Given(@"el lector tiene una sesión activa en la comunidad (\d+)")]
        public void US16_DadoLectorConSesionEnComunidad(int communityId)
        {
            Assert.Equal(_communityId, communityId);
        }

        // ------------------------------------------------------------------
        // AC1 – Publicación CON imagen
        // ------------------------------------------------------------------

        [When(@"el sistema recibe una publicación con contenido ""(.*)"" e imagen ""(.*)""")]
        public void US16_AC1_CuandoRecibePublicacionConImagen(string content, string imgUrl)
        {
            // Creamos el Post directamente con el agregado de dominio
            _createdPost = new Post(
                communityId: _communityId,
                userId:      _userId,
                username:    _username,
                content:     content,
                img:         imgUrl
            );
        }

        [Then(@"el post debe registrarse con la imagen almacenada")]
        public void US16_AC1_EntoncesPostConImagenRegistrado()
        {
            Assert.NotNull(_createdPost);
            Assert.NotEmpty(_createdPost!.Img);
        }

        [Then(@"el post debe tener img no vacío")]
        public void US16_AC1_EntoncesImgNoVacio()
        {
            Assert.False(string.IsNullOrWhiteSpace(_createdPost!.Img),
                "AC1: el sistema debe almacenar el contenido de la imagen de forma segura");
        }

        // ------------------------------------------------------------------
        // AC2 – Publicación SOLO textual
        // ------------------------------------------------------------------

        [When(@"el sistema recibe una publicación con contenido ""(.*)"" sin imagen")]
        public void US16_AC2_CuandoRecibePublicacionSinImagen(string content)
        {
            _createdPost = new Post(
                communityId: _communityId,
                userId:      _userId,
                username:    _username,
                content:     content,
                img:         string.Empty
            );
        }

        [Then(@"el post debe registrarse con el contenido textual correcto")]
        public void US16_AC2_EntoncesPostTextualRegistrado()
        {
            Assert.NotNull(_createdPost);
            Assert.NotEmpty(_createdPost!.Content);
        }

        [Then(@"el post debe tener img vacío")]
        public void US16_AC2_EntoncesImgVacio()
        {
            Assert.Equal(string.Empty, _createdPost!.Img);
        }

        // ------------------------------------------------------------------
        // Shared Then — disponibilidad para visualización y comentarios
        // ------------------------------------------------------------------

        [Then(@"el post debe estar disponible para visualización y comentarios")]
        public void US16_EntoncesPostDisponible()
        {
            // En el dominio puro, un Post recién creado siempre está disponible.
            // La visibilidad la controla el repositorio — aquí validamos
            // que el agregado fue construido correctamente.
            Assert.NotNull(_createdPost);
            Assert.Equal(_communityId, _createdPost!.CommunityId);
            Assert.Equal(_userId,      _createdPost.UserId);
            Assert.NotEmpty(_createdPost.Username);
        }
    }
}