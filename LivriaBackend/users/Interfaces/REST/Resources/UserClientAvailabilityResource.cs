namespace LivriaBackend.users.Interfaces.REST.Resources
{
    public record UserClientAvailabilityResource(bool? EmailAvailable, bool? UsernameAvailable);
}
