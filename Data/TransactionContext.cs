using Microsoft.EntityFrameworkCore;
using jwtapi.Models;

namespace jwtapi.Data
{
    public class TransactionContext : DbContext
    {
        public TransactionContext(DbContextOptions<TransactionContext> options): base(options) { }
        public DbSet<TransactionProd> Transactions { get; set; }
    }
}