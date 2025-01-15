using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public List<Role> Roles { get; set; } = new();
    }
}
