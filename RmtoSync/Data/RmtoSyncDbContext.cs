using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RmtoSync.Data;

public sealed class RmtoSyncDbContext : DbContext
{
    public DbSet<CameraPhotoRecord> CameraPhotoQueue => Set<CameraPhotoRecord>();
    public DbSet<CameraPhotoRow> CameraPhotos => Set<CameraPhotoRow>();
    public DbSet<LineRow> Lines => Set<LineRow>();

    public RmtoSyncDbContext(DbContextOptions<RmtoSyncDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CameraPhotoRecord>(entity =>
        {
            entity.ToView("vw_CameraFullData");
            entity.HasKey(p => p.PhotoId);
            entity.Ignore(p => p.LineCode);

            entity.Property(p => p.VehicleSpeed).HasConversion(FloatToInt);
            entity.Property(p => p.AverageSpeed).HasConversion(FloatToInt);
            entity.Property(p => p.TotalWeight).HasConversion(FloatToInt);
            entity.Property(p => p.VehicleLen).HasConversion(FloatToInt);
            entity.Property(p => p.TotalOverWeight).HasConversion(FloatToInt);
            entity.Property(p => p.FirstToLastAxlesLen).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight1).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight2).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight3).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight4).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight5).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight6).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight7).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight8).HasConversion(FloatToInt);
            entity.Property(p => p.AxleWeight9).HasConversion(FloatToInt);

            entity.Property(p => p.MaxAllowedWeightForClass).HasConversion(FloatToInt);

            entity.Property(p => p.TotalAxles).HasConversion(IntToByte);
            entity.Property(p => p.VehicleClass).HasConversion(IntToByte);
            entity.Property(p => p.CarClass13).HasConversion(IntToByte);

            entity.Property(p => p.OcrScore).HasConversion(FloatToDecimal);
            entity.Property(p => p.Longitude).HasConversion(FloatToDecimal);
            entity.Property(p => p.Latitude).HasConversion(FloatToDecimal);
        });

        modelBuilder.Entity<CameraPhotoRow>(entity =>
        {
            entity.ToTable("CameraPhotos");
            entity.HasKey(p => p.Id);
        });

        modelBuilder.Entity<LineRow>(entity =>
        {
            entity.ToTable("Lines");
            entity.HasKey(l => l.Id);
        });
    }

    private static readonly ValueConverter<int?, double?> FloatToInt = new(
        v => v.HasValue ? (double)v.Value : null,
        v => v.HasValue ? (int)v.Value : null);

    private static readonly ValueConverter<byte?, int?> IntToByte = new(
        v => v.HasValue ? (int)v.Value : null,
        v => v.HasValue ? (byte)v.Value : null);

    private static readonly ValueConverter<decimal?, double?> FloatToDecimal = new(
        v => v.HasValue ? (double)v.Value : null,
        v => v.HasValue ? (decimal)v.Value : null);
}
