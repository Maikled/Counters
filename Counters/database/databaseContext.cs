using Microsoft.EntityFrameworkCore;

namespace Counters.database
{
    public class databaseContext : DbContext
    {
        public DbSet<Counter> Counters { get; set; }

        public databaseContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=data;Username=postgres;Password=admin");
        }
    }
}