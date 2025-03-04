﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSNAuthService.Domain.Options
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public byte[] Secret { get; set; } 
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; } 
    }
}
