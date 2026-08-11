using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence.Configurations
{
    public class AxleConfiguration : IEntityTypeConfiguration<AxleEntity>
    {
        public void Configure(EntityTypeBuilder<AxleEntity> builder)
        {
            builder.ToTable("Axles", "dbo");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedOnAdd();

            builder.Property(a => a.AxleIndex).IsRequired();
            builder.Property(a => a.Weight).IsRequired();

            builder.HasIndex(a => a.VehicleId);

            builder.HasData(
                // Vehicle 1 (3 axles)
                new AxleEntity { Id = 1, VehicleId = 1, AxleIndex = 1, Weight = 8000, TimeMs = 100, Distance = 0, IsOverweight = false },
                new AxleEntity { Id = 2, VehicleId = 1, AxleIndex = 2, Weight = 8200, TimeMs = 250, Distance = 4.2, IsOverweight = false },
                new AxleEntity { Id = 3, VehicleId = 1, AxleIndex = 3, Weight = 8300, TimeMs = 400, Distance = 8.1, IsOverweight = false },

                // Vehicle 2 (2 axles)
                new AxleEntity { Id = 4, VehicleId = 2, AxleIndex = 1, Weight = 9000, TimeMs = 120, Distance = 0, IsOverweight = false },
                new AxleEntity { Id = 5, VehicleId = 2, AxleIndex = 2, Weight = 9000, TimeMs = 300, Distance = 5.6, IsOverweight = true },

                // Vehicle 3 (4 axles)
                new AxleEntity { Id = 6, VehicleId = 3, AxleIndex = 1, Weight = 8000, TimeMs = 90, Distance = 0, IsOverweight = false },
                new AxleEntity { Id = 7, VehicleId = 3, AxleIndex = 2, Weight = 8000, TimeMs = 220, Distance = 4.0, IsOverweight = false },
                new AxleEntity { Id = 8, VehicleId = 3, AxleIndex = 3, Weight = 8000, TimeMs = 350, Distance = 8.0, IsOverweight = false },
                new AxleEntity { Id = 9, VehicleId = 3, AxleIndex = 4, Weight = 8000, TimeMs = 480, Distance = 12.0, IsOverweight = true }
            );
        }
    }
}
