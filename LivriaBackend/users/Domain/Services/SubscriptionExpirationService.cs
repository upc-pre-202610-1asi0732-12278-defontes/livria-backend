using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.users.Domain.Model.Repositories;

public class SubscriptionExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SubscriptionExpirationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
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
    }
}