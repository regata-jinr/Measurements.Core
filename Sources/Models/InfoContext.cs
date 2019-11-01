using Microsoft.EntityFrameworkCore;

namespace Measurements.Core
{
    public class InfoContext : DbContext
    {
        public DbSet<IrradiationInfo> Irradiations { get; set; }
        public DbSet<MeasurementInfo> Measurements { get; set; }
        public DbSet<SessionInfo>     Sessions     { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(SessionControllerSingleton.ConnectionStringBuilder.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeasurementInfo>()
                .HasIndex(c => c.FileSpectra)
                .IsUnique();

            modelBuilder.Entity<SessionInfo>()
                    .HasAlternateKey(c => c.Name);
        }
    }
}
