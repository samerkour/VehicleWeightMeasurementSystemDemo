using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence.Configurations
{
    public class CameraPhotoConfiguration : IEntityTypeConfiguration<CameraPhotosEntity>
    {
        public void Configure(EntityTypeBuilder<CameraPhotosEntity> builder)
        {
            builder.ToTable("CameraPhotos", "dbo");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.FileName).HasMaxLength(255);
            builder.Property(p => p.RelativePath).HasMaxLength(500);
            builder.Property(p => p.FullPath).HasMaxLength(1000);
            builder.Property(p => p.FileHash).HasMaxLength(100);

            builder.Property(p => p.ImportedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.PlateP1).HasMaxLength(10);
            builder.Property(p => p.PlateP2).HasMaxLength(10);
            builder.Property(p => p.PlateP3).HasMaxLength(10);
            builder.Property(p => p.PlateP4).HasMaxLength(10);

            builder.Property(p => p.TerminalLastError).HasMaxLength(4000);

            builder.HasIndex(p => p.VehicleId);
            builder.HasIndex(p => p.CapturedAt);
            builder.HasIndex(p => new { p.TerminalSent, p.TerminalTtoRegistered, p.TerminalImageExpired });

            builder.HasIndex(p => new { p.TerminalImageDeadlineAt, p.Id })
                .HasDatabaseName("IX_CameraPhotos_TerminalImagePending")
                .HasFilter("[TerminalSent] = 0 AND [TerminalTtoRegistered] = 1");

            builder.HasData(
                new CameraPhotosEntity
                {
                    Id = 1,
                    VehicleId = 1,
                    FileName = "car_00001.jpg",
                    RelativePath = "Photos/car_00001.jpg",
                    FullPath = "C:\\Temp\\Snapshots\\car_00001.jpg",
                    FileSizeBytes = 204800,
                    CapturedAt = new DateTime(2025, 1, 10, 8, 30, 0),
                    ImportedAt = new DateTime(2025, 1, 10, 8, 30, 2),
                    PlateP1 = "۱۸",
                    PlateP2 = "س",
                    PlateP3 = "۲۴۴",
                    PlateP4 = "۱۱",
                    TerminalSent = true,
                    TerminalTtoRegistered = true,
                    TerminalImageExpired = false,
                },
                new CameraPhotosEntity
                {
                    Id = 2,
                    VehicleId = 2,
                    FileName = "car_00002.jpg",
                    RelativePath = "Photos/car_00002.jpg",
                    FullPath = "C:\\Temp\\Snapshots\\car_00002.jpg",
                    FileSizeBytes = 185344,
                    CapturedAt = new DateTime(2025, 1, 10, 9, 15, 0),
                    ImportedAt = new DateTime(2025, 1, 10, 9, 15, 2),
                    PlateP1 = "۷۷",
                    PlateP2 = "ب",
                    PlateP3 = "۵۵۵",
                    PlateP4 = "۴۴",
                    TerminalSent = false,
                    TerminalTtoRegistered = false,
                    TerminalImageExpired = false,
                }
            );
        }
    }
}
