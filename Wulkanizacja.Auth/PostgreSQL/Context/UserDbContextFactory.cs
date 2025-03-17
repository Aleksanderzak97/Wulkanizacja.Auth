using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Wulkanizacja.Auth.PostgreSQL.Options;

namespace Wulkanizacja.Auth.PostgreSQL.Context
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        private readonly PostgresOptions _options;

        public UserDbContextFactory(IOptions<PostgresOptions> options)
        {
            _options = options.Value;
        }

        public UserDbContextFactory()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Wulkanizacja.Auth"))
                .AddJsonFile("appsettings.json")
                .Build();

            _options = configuration.GetSection("postgres").Get<PostgresOptions>();
        }

        public UserDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

            optionsBuilder.UseNpgsql(_options.ConnectionString);

            return new UserDbContext(optionsBuilder.Options);
        }
    }
}
