// US12 – BDD Step Definitions
// Proyecto: LivriaBackend.Tests
// Framework: SpecFlow + xUnit
// Cada método mapea exactamente a una línea Gherkin del .feature

using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Aggregates;
using TechTalk.SpecFlow;
using Xunit;

namespace LivriaBackend.Tests.BDD.Steps
{
    [Binding]
    public class US12_RecomendacionesSteps
    {
        // ------------------------------------------------------------------
        // Estado compartido entre pasos dentro del mismo escenario
        // ------------------------------------------------------------------
        private UserClient _client  = null!;
        private Book       _book    = null!;

        // ------------------------------------------------------------------
        // Background
        // ------------------------------------------------------------------

        [Given(@"existe un usuario cliente registrado con suscripción ""(.*)""")]
        public void DadoExisteUnUsuarioClienteRegistrado(string subscription)
        {
            _client = new UserClient(
                display:      "Lector de Prueba",
                username:     "lector_prueba",
                email:        "lector@livria.com",
                icon:         "icon.png",
                phrase:       "Me encantan los libros",
                subscription: subscription
            );
        }

        [Given(@"existe un libro disponible con título ""(.*)"" de género ""(.*)""")]
        public void DadoExisteUnLibroDisponible(string title, string genre)
        {
            _book = new Book(
                title:       title,
                description: "Una obra épica de fantasía",
                author:      "J.R.R. Tolkien",
                stock:       20,
                cover:       "lotr.jpg",
                genre:       genre,
                language:    "español"
            );
        }

        // ------------------------------------------------------------------
        // AC1 – Sin preferencias
        // ------------------------------------------------------------------

        [Given(@"el usuario no tiene preferencias literarias registradas")]
        public void DadoElUsuarioNoTienePreferencias()
        {
            // Estado inicial del UserClient recién creado — no hace falta acción
            // FavoriteBooks y ExclusionBooks ya están vacías por construcción
        }

        [When(@"el sistema evalúa las recomendaciones para ese usuario")]
        public void CuandoElSistemaEvaluaLasRecomendaciones()
        {
            // En el dominio puro no hay acción — la evaluación es implícita
            // Este step existe para cumplir la narrativa Gherkin
        }

        [Then(@"su lista de favoritos debe estar vacía")]
        public void EntoncesLaListaDeFavoritosDebeEstarVacia()
        {
            Assert.Empty(_client.FavoriteBooks);
        }

        [Then(@"su lista de exclusiones debe estar vacía")]
        public void EntoncesLaListaDeExclusionesDebeEstarVacia()
        {
            Assert.Empty(_client.ExclusionBooks);
        }

        // ------------------------------------------------------------------
        // AC2 – Preferencia positiva
        // ------------------------------------------------------------------

        [Given(@"el sistema presenta el libro ""(.*)"" al usuario")]
        public void DadoElSistemaPresentaElLibro(string title)
        {
            // El libro ya fue construido en el Background
            // Validamos que el título coincide para mayor claridad
            Assert.Equal(title, _book.Title);
        }

        [When(@"el usuario indica interés en el libro")]
        public void CuandoElUsuarioIndicaInteres()
        {
            _client.AddFavoriteBook(_book);
        }

        [Then(@"el libro debe aparecer en la lista de favoritos del usuario")]
        public void EntoncesElLibroDebeEstarEnFavoritos()
        {
            Assert.Contains(_book, _client.FavoriteBooks);
        }

        [Then(@"el libro no debe aparecer en la lista de exclusiones del usuario")]
        public void EntoncesElLibroNoDebeEstarEnExclusiones()
        {
            Assert.DoesNotContain(_book, _client.ExclusionBooks);
        }

        // ------------------------------------------------------------------
        // AC3 – Preferencia negativa
        // ------------------------------------------------------------------

        [When(@"el usuario indica desinterés en el libro")]
        public void CuandoElUsuarioIndicaDesinteres()
        {
            _client.AddExclusionBook(_book);
        }

        [Then(@"el libro debe aparecer en la lista de exclusiones del usuario")]
        public void EntoncesElLibroDebeEstarEnExclusiones()
        {
            Assert.Contains(_book, _client.ExclusionBooks);
        }

        [Then(@"el libro no debe aparecer en la lista de favoritos del usuario")]
        public void EntoncesElLibroNoDebeEstarEnFavoritos()
        {
            Assert.DoesNotContain(_book, _client.FavoriteBooks);
        }
    }
}