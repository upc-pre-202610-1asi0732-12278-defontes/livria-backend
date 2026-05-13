# US19 – BDD Feature File
# Framework: SpecFlow + xUnit
# Escenarios tomados directamente de los Acceptance Criteria

Feature: US19 – Buscar libros y contenido en la aplicación
  Como lector
  Quiero utilizar una barra de búsqueda para encontrar libros y autores
  Para acceder fácilmente a contenido de interés sin navegar por toda la plataforma

  Background:
    Given el catálogo del sistema contiene los siguientes libros:
      | Title                              | Author                    | Genre      | Language |
      | El Principito                      | Antoine de Saint-Exupéry  | fiction    | español  |
      | Cien Años de Soledad               | Gabriel García Márquez    | literature | español  |
      | El Amor en los Tiempos del Cólera  | Gabriel García Márquez    | literature | español  |
      | Don Quijote de la Mancha           | Miguel de Cervantes       | literature | español  |

  Scenario: US19_AC1a – Búsqueda por título exacto
    Given el lector inicia una consulta de búsqueda
    When el sistema recibe el término "El Principito"
    Then el sistema debe retornar 1 resultado
    And el resultado debe contener el libro "El Principito"
    And cada resultado debe tener título, autor e imagen no vacíos

  Scenario: US19_AC1b – Búsqueda por título parcial
    Given el lector inicia una consulta de búsqueda
    When el sistema recibe el término "Años"
    Then el sistema debe retornar al menos 1 resultado
    And cada resultado debe tener título, autor e imagen no vacíos

  Scenario: US19_AC1c – Búsqueda sin coincidencias
    Given el lector inicia una consulta de búsqueda
    When el sistema recibe el término "Harry Potter"
    Then el sistema debe retornar una lista vacía de resultados

  Scenario: US19_AC2a – Búsqueda por autor completo
    Given el lector desea encontrar obras de un autor en particular
    When el sistema recibe el nombre del autor "Gabriel García Márquez"
    Then el sistema debe retornar todos los libros de ese autor
    And el número de resultados debe ser 2
    And cada resultado debe pertenecer al autor "Gabriel García Márquez"

  Scenario: US19_AC2b – Búsqueda por apellido parcial del autor
    Given el lector desea encontrar obras de un autor en particular
    When el sistema recibe el nombre del autor "García"
    Then el sistema debe retornar todos los libros de ese autor
    And el número de resultados debe ser 2