using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Domain.Models
{
    public class RefreshToken
    {
        public string Token { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsRevoked { get; set; } = false;
    }
}
