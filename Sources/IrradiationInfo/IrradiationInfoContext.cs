using Microsoft.EntityFrameworkCore;

namespace Measurements.Core
{
    class IrradiationInfoContext : DbContext
 {
        public DbSet<IrradiationInfo> Irradiations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SessionControllerSingleton.ConnectionStringBuilder.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
