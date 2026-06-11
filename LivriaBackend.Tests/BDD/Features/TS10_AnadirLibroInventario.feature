# TS10 – BDD Feature File
# Framework: SpecFlow + xUnit
# Escenarios tomados directamente de los Acceptance Criteria
# Prefijo "TS10 " en steps que puedan colisionar con otras HUs

Feature: TS10 – Añadir un libro al inventario
  Como developer
  Quiero agregar un libro nuevo al inventario de Livria
  Para ampliar el catálogo y aumentar las ventas

  Background:
    Given TS10 el desarrollador está en la sección de gestión de inventario

  Scenario: TS10_AC1 – Formulario presenta todos los campos requeridos
    When TS10 el sistema recibe la solicitud de registro de un nuevo libro
    Then TS10 el sistema debe requerir título del libro
    And TS10 el sistema debe requerir autor del libro
    And TS10 el sistema debe requerir descripción del libro
    And TS10 el sistema debe requerir stock inicial del libro
    And TS10 el sistema debe requerir género del libro
    And TS10 el sistema debe requerir idioma del libro
    And TS10 el sistema debe requerir portada del libro

  Scenario: TS10_AC2a – Registro exitoso de un nuevo libro
    Given TS10 el desarrollador proporciona un libro con título "Clean Code" autor "Robert C. Martin" género "non_fiction" idioma "english" y stock 10
    When TS10 el sistema procesa el alta del nuevo libro
    Then TS10 el libro debe ser creado con estado activo
    And TS10 el libro debe tener precio de venta igual al 165% del precio de compra
    And TS10 el libro debe aparecer en el listado del inventario

  Scenario: TS10_AC2b – Registro rechazado por género inválido
    Given TS10 el desarrollador proporciona un libro con título "Drama Book" autor "Autor Test" género "drama" idioma "english" y stock 5
    When TS10 el sistema procesa el alta del nuevo libro
    Then TS10 el sistema debe rechazar el libro con un error de género inválido

  Scenario: TS10_AC2c – Registro rechazado por idioma inválido
    Given TS10 el desarrollador proporciona un libro con título "French Book" autor "Autor Test" género "fiction" idioma "french" y stock 5
    When TS10 el sistema procesa el alta del nuevo libro
    Then TS10 el sistema debe rechazar el libro con un error de idioma inválido

  Scenario: TS10_AC2d – Registro rechazado por stock negativo
    Given TS10 el desarrollador proporciona un libro con título "Stock Test" autor "Autor Test" género "fiction" idioma "english" y stock -1
    When TS10 el sistema procesa el alta del nuevo libro
    Then TS10 el sistema debe rechazar el libro con un error de stock inválido
