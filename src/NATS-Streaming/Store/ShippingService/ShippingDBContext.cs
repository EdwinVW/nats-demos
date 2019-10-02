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
            optionsBuilder.UseSqlServer("server=localhost,1434;user id=sa;password=8jkGh47hnDw89Haq8LN2;database=Bookstore-Shipping;");
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