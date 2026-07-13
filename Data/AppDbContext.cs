using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Models;

namespace VehicleWeightMeasurementSystemDemo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<VehicleEntity> Vehicles { get; set; }
        public DbSet<AxleEntity> Axles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleEntity>()
                .HasMany(v => v.Axles)
                .WithOne(a => a.Vehicle)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
