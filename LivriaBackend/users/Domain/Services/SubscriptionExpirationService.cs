using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.users.Domain.Model.Repositories;
using Microsoft.Extensions.Logging;

public class SubscriptionExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpirationService> _logger;

    public SubscriptionExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var userClientRepository = scope.ServiceProvider.GetRequiredService<IUserClientRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var allUsers = await userClientRepository.GetAllAsync();
                foreach (var user in allUsers)
                {
                    if (user.IsPaymentOverdue())
                    {
                        user.UpdateSubscription("freeplan");
                        await userClientRepository.UpdateAsync(user);
                    }
                }

                await unitOfWork.CompleteAsync();

                // Corre una vez al día
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo ejecutar la revisión de suscripciones (¿MySQL caído o credenciales?). Reintento en 30 s.");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}