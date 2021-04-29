using Microsoft.EntityFrameworkCore;
using jwtapi.Models;

namespace jwtapi.Data
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options): base(options) { }
        public DbSet<Product> Product { get; set; }
    }
}