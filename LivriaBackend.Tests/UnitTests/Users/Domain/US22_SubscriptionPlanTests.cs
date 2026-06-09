// US22 – Core Entity Unit Test
// Valida la lógica de gestión del plan de suscripción del agregado UserClient:
// actualización a plan de pago (AC1), reversión a plan gratuito (AC2) y
// control del ciclo de pago, sin dependencias externas.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using LivriaBackend.users.Domain.Model.Aggregates;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Users.Domain
{
    public class US22_SubscriptionPlanTests
    {
        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        private static UserClient BuildClient(string subscription = "freeplan") =>
            new UserClient(
                display:      "Test Reader",
                username:     "testreader",
                email:        "reader@livria.com",
                icon:         "icon.png",
                phrase:       "I love books",
                subscription: subscription
            );

        // ==================================================================
        // AC1 – Actualización exitosa a un plan de pago
        // ==================================================================

        [Fact]
        public void US22_AC1_UpdateSubscription_ToCommunityPlan_ShouldChangePlan()
        {
            // Arrange
            var client = BuildClient(subscription: "freeplan");

            // Act
            client.UpdateSubscription("communityplan");

            // Assert
            Assert.Equal("communityplan", client.Subscription);
        }

        [Fact]
        public void US22_AC1_UpdateSubscription_ShouldStampPlanChangeDate()
        {
            // Arrange
            var client = BuildClient();
            var before = DateTime.UtcNow;

            // Act
            client.UpdateSubscription("communityplan");

            // Assert — se registra la fecha del cambio de plan (inicio del ciclo)
            var after = DateTime.UtcNow;
            Assert.NotNull(client.PlanChangeDate);
            Assert.True(client.PlanChangeDate >= before && client.PlanChangeDate <= after);
        }

        [Fact]
        public void US22_AC1_SetHasPayed_WhenTrue_ShouldMarkAsPaidAndResetCycle()
        {
            // Arrange
            var client = BuildClient();
            var before = DateTime.UtcNow;

            // Act — el admin valida el comprobante y marca el pago
            client.SetHasPayed(true);

            // Assert
            Assert.True(client.HasPayed);
            Assert.NotNull(client.PlanChangeDate);
            Assert.True(client.PlanChangeDate >= before);
        }

        [Fact]
        public void US22_AC1_UpdateSubscription_WhenEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var client = BuildClient();

            // Act & Assert — el plan no puede ser vacío
            Assert.Throws<ArgumentException>(() => client.UpdateSubscription("  "));
        }

        // ==================================================================
        // AC2 – Reversión a la suscripción gratuita
        // ==================================================================

        [Fact]
        public void US22_AC2_UpdateSubscription_ToFreePlan_ShouldRevertPlan()
        {
            // Arrange
            var client = BuildClient(subscription: "communityplan");
            client.SetHasPayed(true);

            // Act
            client.UpdateSubscription("freeplan");

            // Assert
            Assert.Equal("freeplan", client.Subscription);
        }

        [Fact]
        public void US22_AC2_UpdateSubscription_ToFreePlan_ShouldResetHasPayed()
        {
            // Arrange — el usuario tenía un plan de pago activo
            var client = BuildClient(subscription: "communityplan");
            client.SetHasPayed(true);

            // Act — vuelve al plan gratuito
            client.UpdateSubscription("freeplan");

            // Assert — se restablece el estado de pago
            Assert.False(client.HasPayed);
        }

        // ==================================================================
        // Control del ciclo de pago — IsPaymentOverdue
        // ==================================================================

        [Fact]
        public void US22_IsPaymentOverdue_WhenFreePlan_ShouldReturnFalse()
        {
            // Arrange
            var client = BuildClient(subscription: "freeplan");

            // Act & Assert — el plan gratuito nunca está en mora
            Assert.False(client.IsPaymentOverdue());
        }

        [Fact]
        public void US22_IsPaymentOverdue_WhenCommunityPlanRecentlyPaid_ShouldReturnFalse()
        {
            // Arrange
            var client = BuildClient(subscription: "communityplan");
            client.SetHasPayed(true); // ciclo reiniciado a hoy

            // Act & Assert — dentro del ciclo de 37 días no hay mora
            Assert.False(client.IsPaymentOverdue());
        }
    }
}
