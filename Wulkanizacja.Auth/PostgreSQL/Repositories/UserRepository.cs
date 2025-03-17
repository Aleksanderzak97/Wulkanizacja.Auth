using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Wulkanizacja.Auth.PostgreSQL.Context;
using Wulkanizacja.Auth.PostgreSQL.Entities;

namespace Wulkanizacja.Auth.PostgreSQL.Repositories
{
    public class UserRepository(UserDbContext userDbContext, IPasswordHasher<UserRecord> passwordHasher) : IUserRepository
    {
        private readonly IPasswordHasher<UserRecord> _passwordHasher = passwordHasher;

        public async Task<UserRecord?> GetUserByUsername(string username, CancellationToken cancellation)
        {
            return await userDbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellation);
        }

        public async Task<UserRecord> Login(UserRecord userRecord, CancellationToken cancellationToken)
        {
            await WaitForFreeTransaction(cancellationToken);

            var user = await GetUserByUsername(userRecord.Username, cancellationToken);
            if(user == null)
            {
                throw new Exception("Błędny login");
            }
            var result = _passwordHasher.VerifyHashedPassword(userRecord, user.Password, userRecord.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Podano błędne hasło");
            }
                return user;
        }

        public async Task<UserRecord> Register(UserRecord userRecord, CancellationToken cancellationToken)
        {
            await WaitForFreeTransaction(cancellationToken);

            await using var transaction = await userDbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var userFromLogin = await userDbContext.Users.FirstOrDefaultAsync(x => x.Username == userRecord.Username, cancellationToken);
                if (userFromLogin != null)
                {
                    throw new Exception("Użytkownik o takim loginie już istnieje");
                }
                var userFromEmail = await userDbContext.Users.FirstOrDefaultAsync(x => x.Email == userRecord.Email, cancellationToken);
                if (userFromEmail != null)
                {
                    throw new Exception("Adres Email jest w użyciu");
                }

                userRecord.Password = _passwordHasher.HashPassword(userRecord, userRecord.Password);
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
