using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<VehicleEntity> Vehicles { get; set; }
        public DbSet<AxleEntity> Axles { get; set; }
        public DbSet<LineEntity> Lines => Set<LineEntity>();

        public DbSet<CameraPhotosEntity> CameraPhotos { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔹 Vehicle → Axles (1:N)
            modelBuilder.Entity<VehicleEntity>()
                .HasMany(v => v.Axles)
                .WithOne(a => a.Vehicle)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Vehicle → CameraPhotos (1:N)
            modelBuilder.Entity<VehicleEntity>()
                .HasMany(v => v.Photos)
                .WithOne(p => p.Vehicle)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            // 🔹 Line → Vehicles (1:N)
            modelBuilder.Entity<LineEntity>()
                .HasMany(l => l.Vehicles)
                .WithOne(v => v.Line)
                .HasForeignKey(v => v.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Unique LineCode
            modelBuilder.Entity<LineEntity>()
                .HasIndex(l => l.LineCode)
                .IsUnique();

            // 🔹 Table Names (optional but recommended)
            modelBuilder.Entity<VehicleEntity>().ToTable("Vehicles");
            modelBuilder.Entity<AxleEntity>().ToTable("Axles");
            modelBuilder.Entity<LineEntity>().ToTable("Lines");
            modelBuilder.Entity<CameraPhotosEntity>().ToTable("CameraPhotos");
        }


    }
}
