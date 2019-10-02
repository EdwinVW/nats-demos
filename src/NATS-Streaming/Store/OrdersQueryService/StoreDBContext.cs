using Microsoft.EntityFrameworkCore;

namespace Store.OrdersQueryService
{
    public class StoreDBContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedProduct> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=localhost,1434;user id=sa;password=8jkGh47hnDw89Haq8LN2;database=Bookstore-ReadModel;");
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