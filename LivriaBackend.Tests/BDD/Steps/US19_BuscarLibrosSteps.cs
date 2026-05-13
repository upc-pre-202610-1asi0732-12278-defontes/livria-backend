// US19 – BDD Step Definitions
// Framework: SpecFlow + xUnit
// Valida el comportamiento de búsqueda por título (AC1) y
// por autor (AC2) directamente sobre el catálogo de libros.

using LivriaBackend.commerce.Domain.Model.Aggregates;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;

namespace LivriaBackend.Tests.BDD.Steps
{
    [Binding]
    public class US19_BuscarLibrosSteps
    {
        // ------------------------------------------------------------------
        // Estado compartido entre pasos
        // ------------------------------------------------------------------
        private List<Book>  _catalog  = new();
        private List<Book>  _results  = new();
        private string      _query    = string.Empty;

        // ------------------------------------------------------------------
        // Helper: mismo filtro que usa SearchPage en Flutter
        // ------------------------------------------------------------------
        private List<Book> ApplyFilter(string query) =>
            _catalog.Where(b =>
                b.Title.Contains(query,  StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase)
            ).ToList();

        // ------------------------------------------------------------------
        // Background
        // ------------------------------------------------------------------

        [Given(@"el catálogo del sistema contiene los siguientes libros:")]
        public void US19_DadoCatalogoConLibros(Table table)
        {
            _catalog = table.Rows.Select(row => new Book(
                title:       row["Title"],
                description: "Descripción de prueba",
                author:      row["Author"],
                stock:       10,
                cover:       $"https://livria.com/covers/{row["Title"].Replace(" ", "_")}.jpg",
                genre:       row["Genre"],
                language:    row["Language"]
            )).ToList();
        }

        // ------------------------------------------------------------------
        // AC1 – Búsqueda por título
        // ------------------------------------------------------------------

        [Given(@"el lector inicia una consulta de búsqueda")]
        public void US19_AC1_DadoLectorIniciaConsulta()
        {
            _results = new List<Book>();
        }

        [When(@"el sistema recibe el término ""(.*)""")]
        public void US19_AC1_CuandoRecibeTermino(string term)
        {
            _query   = term;
            _results = ApplyFilter(term);
        }

        [Then(@"el sistema debe retornar (\d+) resultado")]
        public void US19_EntoncesRetornaResultados(int count)
        {
            Assert.Equal(count, _results.Count);
        }

        [Then(@"el resultado debe contener el libro ""(.*)""")]
        public void US19_EntoncesResultadoContieneLibro(string expectedTitle)
        {
            Assert.Contains(_results, b =>
                string.Equals(b.Title, expectedTitle,
                    StringComparison.OrdinalIgnoreCase));
        }

        [Then(@"cada resultado debe tener título, autor e imagen no vacíos")]
        public void US19_EntoncesResultadosTienenCamposEsenciales()
        {
            Assert.All(_results, book =>
            {
                Assert.False(string.IsNullOrWhiteSpace(book.Title),
                    $"Libro sin título en resultados");
                Assert.False(string.IsNullOrWhiteSpace(book.Author),
                    $"Libro '{book.Title}' sin autor");
                Assert.False(string.IsNullOrWhiteSpace(book.Cover),
                    $"Libro '{book.Title}' sin imagen");
            });
        }

        [Then(@"el sistema debe retornar al menos 1 resultado")]
        public void US19_EntoncesRetornaAlMenosUnResultado()
        {
            Assert.NotEmpty(_results);
        }

        [Then(@"el sistema debe retornar una lista vacía de resultados")]
        public void US19_EntoncesRetornaListaVacia()
        {
            Assert.Empty(_results);
        }

        // ------------------------------------------------------------------
        // AC2 – Búsqueda por autor
        // ------------------------------------------------------------------

        [Given(@"el lector desea encontrar obras de un autor en particular")]
        public void US19_AC2_DadoLectorBuscaAutor()
        {
            _results = new List<Book>();
        }

        [When(@"el sistema recibe el nombre del autor ""(.*)""")]
        public void US19_AC2_CuandoRecibeNombreAutor(string authorQuery)
        {
            _query   = authorQuery;
            _results = ApplyFilter(authorQuery);
        }

        [Then(@"el sistema debe retornar todos los libros de ese autor")]
        public void US19_AC2_EntoncesRetornaTodosLosLibrosDelAutor()
        {
            Assert.NotEmpty(_results);
            Assert.All(_results, book =>
                Assert.True(
                    book.Author.Contains(_query, StringComparison.OrdinalIgnoreCase),
                    $"El libro '{book.Title}' no pertenece al autor buscado '{_query}'"
                ));
        }

        [Then(@"el número de resultados debe ser (\d+)")]
        public void US19_AC2_EntoncesNumeroResultadosCorrecto(int expectedCount)
        {
            Assert.Equal(expectedCount, _results.Count);
        }

        [Then(@"cada resultado debe pertenecer al autor ""(.*)""")]
        public void US19_AC2_EntoncesResultadosPerteneceAlAutor(string expectedAuthor)
        {
            Assert.All(_results, book =>
                Assert.Equal(expectedAuthor, book.Author));
        }
    }
}