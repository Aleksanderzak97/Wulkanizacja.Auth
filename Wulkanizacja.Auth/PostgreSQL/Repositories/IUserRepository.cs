using Wulkanizacja.Auth.PostgreSQL.Entities;

namespace Wulkanizacja.Auth.PostgreSQL.Repositories
{
    public interface IUserRepository
    {
        Task<UserRecord> Login(UserRecord userRecord, CancellationToken cancellation);

        Task<UserRecord> Register(UserRecord userRecord, CancellationToken cancellation);
        Task<UserRecord?> GetUserByUsername(string username, CancellationToken cancellation);

    }
}
