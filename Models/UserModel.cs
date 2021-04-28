using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace jwtapi.Models
{
    public class UserModel : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
    }
}