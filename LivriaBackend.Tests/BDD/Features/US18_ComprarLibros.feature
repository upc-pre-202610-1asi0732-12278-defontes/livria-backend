# US18 – BDD Feature File
# Framework: SpecFlow + xUnit
# Escenario tomado directamente del Acceptance Criteria

Feature: US18 – Comprar libros digitales y físicos
  Como lector
  Quiero poder comprar libros desde la plataforma
  Para acceder a lecturas nuevas de manera inmediata
  O recibir ediciones impresas en mi domicilio

  Background:
    Given existe un cliente con ID 1 y email "lector@livria.com"
    And el carrito contiene los siguientes ítems:
      | BookId | Title              | Author          | Price | Quantity |
      | 1      | El Principito      | Saint-Exupéry   | 29.99 | 2        |
      | 2      | Cien Años Soledad  | García Márquez  | 39.99 | 1        |

  Scenario: US18_AC2a – Compra exitosa sin delivery
    Given el lector ha revisado su carrito y decide completar la compra
    When el lector selecciona "Completar compra" sin envío a domicilio
    Then el sistema debe registrar la orden con los ítems del carrito
    And el total de la orden debe ser 99.97
    And el estado inicial de la orden debe ser "pending"
    And el sistema debe generar un código único de 6 caracteres para la orden

  Scenario: US18_AC2b – Compra exitosa con delivery a zona 1
    Given el lector ha revisado su carrito y decide completar la compra
    When el lector selecciona "Completar compra" con envío a "Miraflores"
    Then el sistema debe registrar la orden con los ítems del carrito
    And el total de la orden debe incluir el costo de envío de 5.00
    And el total final debe ser 104.97
    And el estado inicial de la orden debe ser "pending"

  Scenario: US18_AC2c – Compra rechazada sin ítems en el carrito
    Given el carrito del lector está vacío
    When el lector intenta seleccionar "Completar compra"
    Then el sistema debe rechazar la compra con un error de carrito vacío

  Scenario: US18_AC2d – Compra rechazada por delivery sin shipping details
    Given el lector ha revisado su carrito y decide completar la compra
    When el lector selecciona "Completar compra" con delivery pero sin datos de envío
    Then el sistema debe rechazar la compra con un error de datos de envío