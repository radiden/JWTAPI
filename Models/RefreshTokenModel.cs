using System;

namespace jwtapi.Models
{
    public class RefreshToken
    {
        public string JWT { get; set; }
        public DateTime Expiration { get; set; }
        public string Signature { get; set; }
    }
}
