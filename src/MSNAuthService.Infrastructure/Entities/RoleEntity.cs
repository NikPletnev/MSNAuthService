using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Infrastructure.Entities
{
    public class RoleEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public List<UserEntity> Users { get; set; } = new();
    }
}
