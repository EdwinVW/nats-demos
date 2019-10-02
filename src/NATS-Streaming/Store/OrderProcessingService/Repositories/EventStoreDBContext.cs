using Microsoft.EntityFrameworkCore;

namespace Store.OrderProcessingService.Repositories
{
    public class EventStoreDBContext : DbContext
    {
        public DbSet<OrderAggregate> Orders { get; set; }
        public DbSet<OrderEvent> Events { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=localhost,1434;user id=sa;password=8jkGh47hnDw89Haq8LN2;database=Bookstore-WriteModel;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderEvent>()
                .HasOne(e => e.Order)
                .WithMany(o => o.Events)
                .OnDelete(DeleteBehavior.Cascade);
        }        
    }
}