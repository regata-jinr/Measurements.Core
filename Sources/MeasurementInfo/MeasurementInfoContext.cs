using Microsoft.EntityFrameworkCore;

namespace Measurements.Core
{
    public class MeasurementInfoContext : DbContext
    {
        public DbSet<MeasurementInfo> Measurements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SessionControllerSingleton.ConnectionStringBuilder.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeasurementInfo>()
                .HasAlternateKey(c => c.FileSpectra);
        }
    }
}
