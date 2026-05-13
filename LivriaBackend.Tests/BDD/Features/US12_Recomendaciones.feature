# US12 – BDD Feature File
# Proyecto: LivriaBackend.Tests
# Framework: SpecFlow + xUnit
# Los escenarios reflejan exactamente los Acceptance Criteria de la HU

Feature: US12 – Interactuar con recomendaciones según preferencias literarias
  Como lector
  Quiero recibir recomendaciones personalizadas basadas en mis preferencias literarias
  Para poder descubrir nuevos libros y autores

  Background:
    Given existe un usuario cliente registrado con suscripción "freeplan"
    And existe un libro disponible con título "El Señor de los Anillos" de género "fiction"

  Scenario: AC1 – Generación de recomendaciones iniciales sin preferencias
    Given el usuario no tiene preferencias literarias registradas
    When el sistema evalúa las recomendaciones para ese usuario
    Then su lista de favoritos debe estar vacía
    And su lista de exclusiones debe estar vacía

  Scenario: AC2 – Registro de preferencia positiva
    Given el sistema presenta el libro "El Señor de los Anillos" al usuario
    When el usuario indica interés en el libro
    Then el libro debe aparecer en la lista de favoritos del usuario
    And el libro no debe aparecer en la lista de exclusiones del usuario

  Scenario: AC3 – Registro de preferencia negativa
    Given el sistema presenta el libro "El Señor de los Anillos" al usuario
    When el usuario indica desinterés en el libro
    Then el libro debe aparecer en la lista de exclusiones del usuario
    And el libro no debe aparecer en la lista de favoritos del usuario