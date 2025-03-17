namespace Wulkanizacja.Auth.PostgreSQL.Services
{
    public interface IDatabaseMigrationService
    {
        Task EnsureMigrationsAppliedAsync(CancellationToken cancellationToken = default);
    }
}
