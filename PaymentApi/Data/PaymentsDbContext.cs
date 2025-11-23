using Microsoft.EntityFrameworkCore;
using PaymentApi.Models;

namespace PaymentApi.Data
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ReferenceCounter> ReferenceCounters => Set<ReferenceCounter>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ClientRequestId)
                .IsUnique();
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.Reference)
                .IsUnique();

            modelBuilder.Entity<ReferenceCounter>()
                .HasKey(rc => rc.DateKey);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
