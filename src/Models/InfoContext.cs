using Microsoft.EntityFrameworkCore;

namespace Regata.Measurements.Models
{
  public class InfoContext : DbContext
  {
    public DbSet<IrradiationInfo> Irradiations { get; set; }
    public DbSet<MeasurementInfo> Measurements { get; set; }

    private readonly string _conString;

    public InfoContext(string conStr) : base()
    {
      _conString = conStr;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlServer(_conString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<MeasurementInfo>()
          .HasIndex(c => c.FileSpectra)
          .IsUnique();

    }
  }
}