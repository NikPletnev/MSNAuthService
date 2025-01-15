using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Infrastructure.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public List<RoleEntity> Roles { get; set; } = new();
    }
}
