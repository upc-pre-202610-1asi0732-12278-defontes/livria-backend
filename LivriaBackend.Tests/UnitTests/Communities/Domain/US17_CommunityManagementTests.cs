// US17 – Core Entity Unit Test
// Valida la lógica de gestión de comunidades: creación y validación del
// agregado Community (AC1) y asignación de membresía a través de
// UserClient.JoinCommunity / LeaveCommunity (AC2), sin dependencias externas.
// Framework: xUnit | Patrón: Arrange – Act – Assert

using System.Linq;
using LivriaBackend.communities.Domain.Model.Aggregates;
using LivriaBackend.communities.Domain.Model.ValueObjects;
using LivriaBackend.users.Domain.Model.Aggregates;
using Xunit;

namespace LivriaBackend.Tests.UnitTests.Communities.Domain
{
    public class US17_CommunityManagementTests
    {
        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        private static Community BuildCommunity(
            string        name        = "Club de Lectura",
            string        description = "Comunidad para amantes de la literatura",
            CommunityType type        = CommunityType.literature,
            int           ownerId     = 1) =>
            new Community(name, description, type, ownerId);

        private static UserClient BuildClient(string username = "lector01") =>
            new UserClient(
                display:      "Test Reader",
                username:     username,
                email:        "reader@livria.com",
                icon:         "icon.png",
                phrase:       "I love books",
                subscription: "communityplan"
            );

        // ==================================================================
        // AC1 – Creación y registro de una nueva comunidad
        // ==================================================================

        [Fact]
        public void US17_AC1_CreateCommunity_WhenValid_ShouldSetProperties()
        {
            // Arrange & Act
            var community = BuildCommunity(
                name:        "Club Sci-Fi",
                description: "Ciencia ficción y fantasía",
                type:        CommunityType.fiction,
                ownerId:     7);

            // Assert
            Assert.Equal("Club Sci-Fi",                community.Name);
            Assert.Equal("Ciencia ficción y fantasía", community.Description);
            Assert.Equal(CommunityType.fiction,        community.Type);
            Assert.Equal(7,                            community.OwnerId);
        }

        [Fact]
        public void US17_AC1_CreateCommunity_WhenNameIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => BuildCommunity(name: ""));
        }

        [Fact]
        public void US17_AC1_CreateCommunity_WhenDescriptionIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => BuildCommunity(description: "  "));
        }

        [Fact]
        public void US17_AC1_UpdateName_WhenEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var community = BuildCommunity();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => community.UpdateName(""));
        }

        [Fact]
        public void US17_AC1_Update_ShouldChangeAllEditableFields()
        {
            // Arrange
            var community = BuildCommunity();

            // Act
            community.Update(
                newName:        "Nuevo Nombre",
                newDescription: "Nueva descripción",
                newType:        CommunityType.juvenile,
                newImage:       "img.png",
                newBanner:      "banner.png");

            // Assert
            Assert.Equal("Nuevo Nombre",          community.Name);
            Assert.Equal("Nueva descripción",     community.Description);
            Assert.Equal(CommunityType.juvenile,  community.Type);
            Assert.Equal("img.png",               community.Image);
            Assert.Equal("banner.png",            community.Banner);
        }

        // ==================================================================
        // AC2 – Asignación de membresía a una comunidad
        // ==================================================================

        [Fact]
        public void US17_AC2_JoinCommunity_ShouldRegisterMembership()
        {
            // Arrange
            var client = BuildClient();

            // Act
            client.JoinCommunity(communityId: 10);

            // Assert
            Assert.Single(client.UserCommunities);
            Assert.Contains(client.UserCommunities, uc => uc.CommunityId == 10);
        }

        [Fact]
        public void US17_AC2_JoinCommunity_WhenAlreadyMember_ShouldNotDuplicate()
        {
            // Arrange
            var client = BuildClient();

            // Act — se intenta unir dos veces a la misma comunidad
            client.JoinCommunity(10);
            client.JoinCommunity(10);

            // Assert
            Assert.Single(client.UserCommunities);
        }

        [Fact]
        public void US17_AC2_LeaveCommunity_ShouldRemoveMembership()
        {
            // Arrange
            var client = BuildClient();
            client.JoinCommunity(10);

            // Act
            client.LeaveCommunity(10);

            // Assert
            Assert.DoesNotContain(client.UserCommunities, uc => uc.CommunityId == 10);
        }

        [Fact]
        public void US17_AC2_AddUser_ShouldRegisterMemberInCommunity()
        {
            // Arrange
            var community = BuildCommunity();
            var client    = BuildClient();

            // Act
            community.AddUser(client);

            // Assert
            Assert.Single(community.UserCommunities);
        }

        [Fact]
        public void US17_AC2_AddUser_WhenAlreadyMember_ShouldNotDuplicate()
        {
            // Arrange
            var community = BuildCommunity();
            var client    = BuildClient();

            // Act
            community.AddUser(client);
            community.AddUser(client);

            // Assert
            Assert.Single(community.UserCommunities);
        }

        [Fact]
        public void US17_AC2_AddUser_WhenNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var community = BuildCommunity();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => community.AddUser(null));
        }

        // ==================================================================
        // Anonimización del dueño (eliminación de cuenta)
        // ==================================================================

        [Fact]
        public void US17_AnonymizeOwner_ShouldReassignOwnerToDeletedUser()
        {
            // Arrange
            var community = BuildCommunity(ownerId: 99);

            // Act
            community.AnonymizeOwner(UserConstants.DeletedUserId);

            // Assert
            Assert.Equal(UserConstants.DeletedUserId, community.OwnerId);
        }
    }
}
