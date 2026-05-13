using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.IAM.Domain.Model.Commands;
using LivriaBackend.IAM.Application.Resources;
using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Commands;
using LivriaBackend.users.Domain.Model.Repositories;

namespace LivriaBackend.users.Application.Internal.CommandServices
{
    public class RegisterUserClientCompositeCommandHandler : IRequestHandler<RegisterUserClientCompositeCommand, UserClient>
    {
        private readonly IUserClientRepository _userClientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public RegisterUserClientCompositeCommandHandler(
            IUserClientRepository userClientRepository,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _userClientRepository = userClientRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<UserClient> Handle(RegisterUserClientCompositeCommand command, CancellationToken cancellationToken)
        {
            if (await _userClientRepository.ExistsByUsernameAsync(command.Username))
            {
                throw new ArgumentException($"User with username '{command.Username}' already exists.");
            }

            if (await _userClientRepository.ExistsByEmailAsync(command.Email))
            {
                throw new ArgumentException($"User with email '{command.Email}' already exists.");
            }

            var userClient = new UserClient(
                command.Display,
                command.Username,
                command.Email,
                command.Icon,
                command.Phrase,
                "freeplan"
            );
            
            await _userClientRepository.AddAsync(userClient);
            
            await _unitOfWork.CompleteAsync();
            
            if (userClient.Id == 0)
            {
                throw new ApplicationException("Failed to generate UserClient ID during registration.");
            }
            
            var registerIamCommand = new RegisterCommand(
                userClient.Id,
                command.Username,
                command.Password
            );
            
            await _mediator.Send(registerIamCommand, cancellationToken);
        
            return userClient;
        }
    }
}
