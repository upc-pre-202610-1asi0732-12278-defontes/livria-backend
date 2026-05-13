# US20 – BDD Feature File
# Framework: SpecFlow + xUnit
# TODOS los steps del Background llevan prefijo "US20 "
# para evitar colisión con US18_ComprarLibrosSteps

Feature: US20 – Gestionar pago de libros
  Como lector
  Quiero poder pagar mis libros mediante transacción bancaria
  Para asegurarme de que mi compra sea rápida y confiable

  Background:
    Given US20 existe un cliente con ID 1 y email "lector@livria.com"
    And US20 el lector tiene una orden con 1 ítem de precio 29.99

  Scenario: US20_AC1a – Sistema muestra CCI para la transferencia
    Given US20 el lector ha completado los pasos de información de compra
    When US20 el lector selecciona continuar con el proceso de pago
    Then el sistema debe tener disponible el número de cuenta interbancaria
    And el CCI debe tener 18 dígitos numéricos
    And la orden debe mostrar el monto total a transferir mayor a 0

  Scenario: US20_AC1b – Total con envío a zona 1 incluido
    Given US20 el lector ha completado los pasos de información de compra
    And el lector seleccionó envío a domicilio en "Miraflores"
    When US20 el lector selecciona continuar con el proceso de pago
    Then US20 el total de la orden debe incluir el costo de envío de 5.00
    And US20 el total final debe ser 34.99

  Scenario: US20_AC2a – Orden queda en estado pending tras adjuntar comprobante
    Given US20 el lector ha realizado la transferencia bancaria
    When el sistema recibe la orden con comprobante adjunto
    Then US20 la orden debe crearse con estado "pending"
    And el código de la orden debe ser alfanumérico de 6 caracteres
    And el sistema debe poder actualizar el estado a "in progress" cuando el admin valide

  Scenario: US20_AC2b – Orden rechaza estado inválido
    Given US20 el lector ha realizado la transferencia bancaria
    When el sistema recibe la orden con comprobante adjunto
    Then el sistema debe rechazar el estado "verificado" como inválido
