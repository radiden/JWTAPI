using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using jwtapi.Models;

namespace jwtapi.Data
{
    public class UserContext : IdentityDbContext<UserModel>
    {
        public UserContext(DbContextOptions<UserContext> options): base(options) { }
    }
}