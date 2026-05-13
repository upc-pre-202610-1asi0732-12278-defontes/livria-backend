# US16 – BDD Feature File
# Framework: SpecFlow + xUnit
# Escenarios tomados directamente de los Acceptance Criteria

Feature: US16 – Crear publicaciones en comunidades
  Como lector
  Quiero poder crear y compartir publicaciones dentro de las comunidades temáticas
  Para interactuar con otros lectores a través de imágenes y texto

  Background:
    Given existe una comunidad con ID 1 en el sistema
    And existe un usuario con ID 42 y username "lector01" con sesión activa

  Scenario: US16_AC1 – Creación de publicación con imagen
    Given el lector tiene una sesión activa en la comunidad 1
    When el sistema recibe una publicación con contenido "Miren esta portada" e imagen "https://livria.com/uploads/img.jpg"
    Then el post debe registrarse con la imagen almacenada
    And el post debe estar disponible para visualización y comentarios
    And el post debe tener img no vacío

  Scenario: US16_AC2 – Creación de publicación solo textual
    Given el lector tiene una sesión activa en la comunidad 1
    When el sistema recibe una publicación con contenido "Acabo de terminar Cien Años de Soledad" sin imagen
    Then el post debe registrarse con el contenido textual correcto
    And el post debe estar disponible para visualización y comentarios
    And el post debe tener img vacío