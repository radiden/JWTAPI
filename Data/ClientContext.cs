using Microsoft.EntityFrameworkCore;
using jwtapi.Models;

namespace jwtapi.Data
{
    public class ClientContext : DbContext
    {
        public ClientContext(DbContextOptions<ClientContext> options): base(options) { }
        public DbSet<Client> Client { get; set; }
    }
}