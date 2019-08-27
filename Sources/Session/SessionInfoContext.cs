using Microsoft.EntityFrameworkCore;

namespace Measurements.Core
    {
        public class SessionInfoContext : DbContext
        {
            public DbSet<SessionInfo> Sessions { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(SessionControllerSingleton.ConnectionStringBuilder.ConnectionString);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<SessionInfo>()
                    .HasAlternateKey(c => c.Name);
            }
        }
}



