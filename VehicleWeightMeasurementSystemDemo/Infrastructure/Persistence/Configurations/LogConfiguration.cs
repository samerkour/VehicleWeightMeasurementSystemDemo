using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence.Configurations
{
    public class LogConfiguration : IEntityTypeConfiguration<LogEntity>
    {
        public void Configure(EntityTypeBuilder<LogEntity> builder)
        {
            builder.ToTable("Logs", "dbo");

            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).ValueGeneratedOnAdd();

            builder.Property(l => l.Message).HasMaxLength(4000);
            builder.Property(l => l.MessageTemplate).HasMaxLength(4000);
            builder.Property(l => l.Level).HasMaxLength(50);
            builder.Property(l => l.Exception).HasMaxLength(4000);
            builder.Property(l => l.Properties).HasMaxLength(4000);
            builder.Property(l => l.LogEvent).HasMaxLength(4000);
        }
    }
}
