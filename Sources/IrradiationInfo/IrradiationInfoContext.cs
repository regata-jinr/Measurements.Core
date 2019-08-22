using Microsoft.EntityFrameworkCore;

namespace Measurements.Core
{
    class IrradiationInfoContext : DbContext
 {
        public DbSet<IrradiationInfo> Irradiations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
