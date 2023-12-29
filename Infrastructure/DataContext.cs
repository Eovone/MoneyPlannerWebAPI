using Entity;
using Microsoft.EntityFrameworkCore;

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
                .WithOne(i => i.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Expenses)
                .WithOne(e => e.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.MonthAnalysis)
                .WithOne(ma => ma.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Income>()
                .HasMany(i => i.MonthAnalysis)
                .WithMany(ma => ma.Incomes);

            modelBuilder.Entity<Expense>()
                .HasMany(e => e.MonthAnalysis)
                .WithMany(ma => ma.Expenses);
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Income> Incomes { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<MonthAnalysis> MonthAnalysis { get; set; }
        public virtual DbSet<BudgetPlan> BudgetPlans { get; set; }
        public virtual DbSet<BudgetPlanItem> BudgetPlansItems { get; set; }
    }
}
