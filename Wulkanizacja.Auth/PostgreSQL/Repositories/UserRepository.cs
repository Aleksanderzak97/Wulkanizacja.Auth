using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using Wulkanizacja.Auth.PostgreSQL.Context;
using Wulkanizacja.Auth.PostgreSQL.Entities;

namespace Wulkanizacja.Auth.PostgreSQL.Repositories
{
    public class UserRepository(UserDbContext userDbContext) : IUserRepository
    {
        public async Task<UserRecord> Login(UserRecord userRecord, CancellationToken cancellationToken)
        {
            await WaitForFreeTransaction(cancellationToken);

            try
            {
                var user = await userDbContext.Users.FirstOrDefaultAsync(x => x.Username == userRecord.Username, cancellationToken);
                if (user == null)
                {
                    return null;
                }
                if (user.Password != userRecord.Password)
                {
                    return null;
                }
                return user;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<UserRecord> Register(UserRecord userRecord, CancellationToken cancellationToken)
        {
            await WaitForFreeTransaction(cancellationToken);

            await using var transaction = await userDbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await userDbContext.Users.AddAsync(userRecord, cancellationToken);
                await SaveContextChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return userRecord;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task SaveContextChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await userDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task WaitForFreeTransaction(CancellationToken cancellationToken)
        {
            while (userDbContext.Database.CurrentTransaction != null)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
