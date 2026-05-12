using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.users.Interfaces.REST.Resources
{
    public record UpdateUserClientHasPayedResource(
        [Required] bool HasPayed
    );
}