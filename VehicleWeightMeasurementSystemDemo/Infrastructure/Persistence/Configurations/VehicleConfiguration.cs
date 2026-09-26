using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<VehicleEntity>
    {
        public void Configure(EntityTypeBuilder<VehicleEntity> builder)
        {
            builder.ToTable("Vehicles", "dbo");

            builder.HasKey(v => v.Id);
            builder.Property(v => v.Id).ValueGeneratedOnAdd();

            builder.Property(v => v.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(v => v.PlateNumber).HasMaxLength(50);

            builder.Property(v => v.ADC1).HasColumnType("varchar(16)");
            builder.Property(v => v.ADC2).HasColumnType("varchar(16)");
            builder.Property(v => v.ADC3).HasColumnType("varchar(16)");
            builder.Property(v => v.ADC4).HasColumnType("varchar(16)");

            // 🔥 مقادیر آستانه در لحظه ثبت رکورد (Snapshot of config at record time)
            // Defaults با مقادیر appsettings → VehicleClassification هم‌راستا هستند:
            //   WeightViolationThresholdKg = 44000           → MaxAllowedWeightForClass
            //   LightVehicleSpeedViolationThresholdKmh = 110 → MaxAllowedSpeedForClass
            // DEFAULT constraint باعث می‌شود ردیف‌های موجود در ALTER هم به‌صورت خودکار مقدار بگیرند
            // و هیچ‌کدام NULL یا از دست نروند (بدون از دست رفتن داده).
            builder.Property(v => v.MaxAllowedWeightForClass)
                .HasDefaultValue(44000.0);   // = WeightViolationThresholdKg

            builder.Property(v => v.MaxAllowedSpeedForClass)
                .HasDefaultValue(110);        // = LightVehicleSpeedViolationThresholdKmh

            builder.HasIndex(v => v.Timestamp);

            // Vehicle (1) -> (many) Axles — cascade
            builder.HasMany(v => v.Axles)
                .WithOne(a => a.Vehicle)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vehicle (1) -> (many) CameraPhotos
            builder.HasMany(v => v.Photos)
                .WithOne(p => p.Vehicle)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Vehicle -> Line (FK)
            builder.HasOne(v => v.Line)
                .WithMany(l => l.Vehicles)
                .HasForeignKey(v => v.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new VehicleEntity
                {
                    Id = 1,
                    Timestamp = new DateTime(2025, 1, 10, 8, 30, 0),
                    PlateNumber = "\u200F۱۱ ایران ۲۴۴ س ۱۸",
                    Speed = 62.5,
                    AverageSpeed = 60.0,
                    AxleCount = 3,
                    TotalWeight = 24500,
                    LineId = 2,
                    PlateConfidence = 92.5,
                    PlateReadStatus = 1,
                    PlateReadAt = new DateTime(2025, 1, 10, 8, 30, 1),
                    Allowed = true,
                    WrongDirection = false,
                    VehicleClass = 2,
                    VehicleLen = 12.4,
                    TotalOverWeight = 0,
                    MaxAllowedWeightForClass = 44000,
                    MaxAllowedSpeedForClass = 110
                },
                new VehicleEntity
                {
                    Id = 2,
                    Timestamp = new DateTime(2025, 1, 10, 9, 15, 0),
                    PlateNumber = "\u200F۴۴ ایران ۵۵۵ ب ۷۷",
                    Speed = 55.0,
                    AverageSpeed = 52.0,
                    AxleCount = 2,
                    TotalWeight = 18000,
                    LineId = 1,
                    PlateConfidence = 88.0,
                    PlateReadStatus = 1,
                    PlateReadAt = new DateTime(2025, 1, 10, 9, 15, 1),
                    Allowed = true,
                    WrongDirection = false,
                    VehicleClass = 3,
                    VehicleLen = 9.8,
                    TotalOverWeight = 500,
                    MaxAllowedWeightForClass = 44000,
                    MaxAllowedSpeedForClass = 110
                },
                new VehicleEntity
                {
                    Id = 3,
                    Timestamp = new DateTime(2025, 1, 10, 10, 5, 0),
                    PlateNumber = "\u200F۳۳ ایران ۱۱۲ ب ۹۹",
                    Speed = 48.0,
                    AverageSpeed = 45.0,
                    AxleCount = 4,
                    TotalWeight = 32000,
                    LineId = 4,
                    PlateConfidence = 75.5,
                    PlateReadStatus = 1,
                    PlateReadAt = new DateTime(2025, 1, 10, 10, 5, 1),
                    Allowed = false,
                    WrongDirection = false,
                    VehicleClass = 4,
                    VehicleLen = 15.2,
                    TotalOverWeight = 3500,
                    MaxAllowedWeightForClass = 44000,
                    MaxAllowedSpeedForClass = 110
                }
            );
        }
    }
}
