# US13 – BDD Feature File
# Framework: SpecFlow + xUnit
# Escenarios tomados directamente de los Acceptance Criteria

Feature: US13 – Registrar un usuario e iniciar sesión
  Como lector
  Quiero poder registrarme e iniciar sesión con mis credenciales
  Para acceder a la plataforma y descubrir nuevos títulos de mi agrado

  Scenario: AC1 – Creación exitosa de cuenta con credenciales válidas
    Given el sistema recibe una solicitud de registro con username "lector01" y password "SecurePass123"
    When el sistema procesa la creación de la identidad
    Then la identidad debe ser creada correctamente
    And la contraseña debe almacenarse de forma segura sin texto plano
    And el sistema debe poder verificar la contraseña original

  Scenario: AC1b – Registro rechazado por username demasiado corto
    Given el sistema recibe una solicitud de registro con username "ab" y password "SecurePass123"
    When el sistema intenta crear la identidad
    Then el sistema debe rechazar el registro con un error de validación

  Scenario: AC2 – Autenticación exitosa con credenciales correctas
    Given existe una identidad registrada con username "lector01" y password "SecurePass123"
    When el sistema recibe una solicitud de login con username "lector01" y password "SecurePass123"
    Then el sistema debe verificar las credenciales exitosamente
    And el resultado de la verificación debe ser verdadero

  Scenario: AC2b – Autenticación fallida con contraseña incorrecta
    Given existe una identidad registrada con username "lector01" y password "SecurePass123"
    When el sistema recibe una solicitud de login con username "lector01" y password "WrongPass!"
    Then el resultado de la verificación debe ser falso