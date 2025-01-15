using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSNAuthService.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Infrastructure.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.HasKey(roleEntity => roleEntity.Id);
            builder.HasData(
                    new RoleEntity { Id = Guid.NewGuid(), Name = "Admin" },
                    new RoleEntity { Id = Guid.NewGuid(), Name = "User" }
                );
        }
    }
}
