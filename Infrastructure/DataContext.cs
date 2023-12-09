using Entity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Infrastructure
{
    public class DataContext : DbContext
    {
        public DataContext() {}
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Incomes)
                .WithOne(u => u.User);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Expenses)
                .WithOne(u => u.User);                
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Income> Incomes { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
    }
}
