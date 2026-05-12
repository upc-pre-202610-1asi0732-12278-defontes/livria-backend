namespace LivriaBackend.users.Domain.Model.Commands
{
    public record UpdateUserClientHasPayedCommand(
        int UserClientId,
        bool HasPayed
    );
}