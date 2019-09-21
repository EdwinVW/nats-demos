using Microsoft.EntityFrameworkCore;

namespace Store.OrdersQueryService
{
    public class StoreDBContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedProduct> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ReadModel.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderedProduct>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Products)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}