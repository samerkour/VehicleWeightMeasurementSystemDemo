using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence.Configurations
{
    public class LineConfiguration : IEntityTypeConfiguration<LineEntity>
    {
        public void Configure(EntityTypeBuilder<LineEntity> builder)
        {
            builder.ToTable("Lines", "dbo");

            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).ValueGeneratedOnAdd();

            builder.Property(l => l.LineCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(l => l.LineName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(l => l.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(l => l.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(l => l.LineCode).IsUnique();

            builder.HasData(
                new LineEntity
                {
                    Id = 1,
                    LineCode = "L1",
                    LineName = "Line1",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0)
                },
                new LineEntity
                {
                    Id = 2,
                    LineCode = "L2",
                    LineName = "Line2",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0)
                },
                new LineEntity
                {
                    Id = 3,
                    LineCode = "L3",
                    LineName = "Line3",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0)
                }
                ,
                new LineEntity
                {
                    Id = 4,
                    LineCode = "L4",
                    LineName = "Line4",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0)
                }
            );
        }
    }
}
