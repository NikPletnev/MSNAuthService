using Microsoft.EntityFrameworkCore;
using MSNAuthService.Domain.Models;
using MSNAuthService.Infrastructure.Configurations;
using MSNAuthService.Infrastructure.Entities;

namespace MSNAuthService.Infrastructure
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
