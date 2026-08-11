using Microsoft.EntityFrameworkCore;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<VehicleEntity> Vehicles { get; set; }
        public DbSet<AxleEntity> Axles { get; set; }
        public DbSet<LineEntity> Lines => Set<LineEntity>();
        public DbSet<CameraPhotosEntity> CameraPhotos { get; set; }
        public DbSet<LogEntity> Logs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
