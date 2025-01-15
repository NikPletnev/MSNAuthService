using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSNAuthService.Domain.Models;
using MSNAuthService.Infrastructure.Entities;
using System.Reflection.Emit;

namespace MSNAuthService.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(userEntity => userEntity.Id);
            builder.HasMany(u => u.Roles).WithMany();
        }
    }
}
