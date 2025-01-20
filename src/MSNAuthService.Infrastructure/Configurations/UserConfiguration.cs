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

            builder.HasMany(u => u.Roles)
                   .WithMany(r => r.Users)
                   .UsingEntity<Dictionary<string, object>>(
                        "UsersRoles", 
                        j => j.HasOne<RoleEntity>() 
                              .WithMany()
                              .HasForeignKey("RoleId")
                              .HasConstraintName("FK_UsersRoles_Roles"),
                        j => j.HasOne<UserEntity>() 
                              .WithMany()
                              .HasForeignKey("UserId")
                              .HasConstraintName("FK_UsersRoles_Users"));
        }
    }
}
