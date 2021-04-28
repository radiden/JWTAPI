using Microsoft.EntityFrameworkCore;
using jwtapi.Models;

namespace jwtapi.Data
{
    public class RefreshContext : DbContext
    {
        public RefreshContext(DbContextOptions<RefreshContext> options): base(options) { }
        public DbSet<Refresh> Refresh { get; set; }
    }
}
