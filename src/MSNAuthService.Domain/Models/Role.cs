using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Domain.Models
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
    }
}
