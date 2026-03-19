using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var user = await GetUserByUsername(userRecord.Username, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Błędny login");
            }

            var result = _passwordHasher.VerifyHashedPassword(userRecord, user.Password, userRecord.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Podano błędne hasło");
            }

            return user;
        }

        public async Task<UserRecord> Register(UserRecord userRecord, CancellationToken cancellationToken)
        {
            var userFromLogin = await userDbContext.Users.FirstOrDefaultAsync(x => x.Username == userRecord.Username, cancellationToken);
            if (userFromLogin != null)
            {
                throw new InvalidOperationException("Użytkownik o takim loginie już istnieje");
            }

            var userFromEmail = await userDbContext.Users.FirstOrDefaultAsync(x => x.Email == userRecord.Email, cancellationToken);
            if (userFromEmail != null)
            {
                throw new InvalidOperationException("Adres Email jest w użyciu");
            }

            userRecord.Password = _passwordHasher.HashPassword(userRecord, userRecord.Password);
            await userDbContext.Users.AddAsync(userRecord, cancellationToken);
            await userDbContext.SaveChangesAsync(cancellationToken);

            return userRecord;
        }
    }
}
