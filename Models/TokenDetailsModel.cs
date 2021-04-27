using System;
using System.ComponentModel.DataAnnotations;

namespace jwtapi.Models
{
    public class TokenDetails
    {
        [Required]
        public string Token { get; set; }
        public DateTime? TokenExpirationDate { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        public DateTime? RefreshExpirationDate { get; set; }
    }
}
