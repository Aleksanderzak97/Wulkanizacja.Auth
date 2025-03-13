using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Wulkanizacja.Auth.PostgreSQL.Entities;

namespace Wulkanizacja.Auth.PostgreSQL.Context
{
    public class UserDbContext : DbContext
    {
        public DbSet<UserRecord> Users { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRecord>().HasKey(x => x.UserId);
            modelBuilder.Entity<UserRecord>().Property(x => x.Name).IsRequired();
            modelBuilder.Entity<UserRecord>().Property(x => x.Surname).IsRequired();
            modelBuilder.Entity<UserRecord>().HasIndex(x => x.Username).IsUnique();
            modelBuilder.Entity<UserRecord>().Property(x => x.Password).IsRequired();
            modelBuilder.Entity<UserRecord>().HasIndex(x => x.Email).IsUnique();

        }
    }
}
