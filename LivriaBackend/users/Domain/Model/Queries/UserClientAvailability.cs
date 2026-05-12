namespace LivriaBackend.users.Domain.Model.Queries
{
    /// <summary>
    /// Indicates whether requested identifiers are free for registration.
    /// Null means that identifier was not checked.
    /// </summary>
    public sealed class UserClientAvailability
    {
        public bool? EmailAvailable { get; init; }
        public bool? UsernameAvailable { get; init; }
    }
}
