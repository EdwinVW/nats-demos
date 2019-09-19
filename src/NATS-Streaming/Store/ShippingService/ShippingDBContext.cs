using Microsoft.EntityFrameworkCore;

namespace Store.ShippingService
{
    public class StoreDBContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedProduct> Products { get; set; }
        public DbSet<ShippingInfo> ShippingInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Shipping.db");
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