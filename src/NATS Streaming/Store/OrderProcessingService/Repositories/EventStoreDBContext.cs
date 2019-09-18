using Microsoft.EntityFrameworkCore;

namespace Store.OrderProcessingService.Repositories
{
    public class EventStoreDBContext : DbContext
    {
        public DbSet<OrderAggregate> Orders { get; set; }
        public DbSet<OrderEvent> Events { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=WriteModel.db");
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