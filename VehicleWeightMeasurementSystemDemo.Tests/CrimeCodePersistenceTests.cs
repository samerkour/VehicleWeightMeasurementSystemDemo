using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence;
using Xunit;

namespace VehicleWeightMeasurementSystemDemo.Tests
{
    /// <summary>
    /// Verifies that <see cref="SqlRepository.SaveAsync"/> actually writes
    /// <c>Vehicles.CrimeCodes</c> and the calibrated <c>TotalWeight</c>, and that a
    /// later update of other columns does not clear the stored codes.
    ///
    /// The production app is SQL Server only; the in-memory provider is used here to
    /// exercise the repository mapping without touching a real database.
    /// </summary>
    public class CrimeCodePersistenceTests
    {
        [Fact]
        public async Task SaveAsync_PersistsCrimeCodesAndCalibratedTotalWeight()
        {
            var options = CreateOptions();
            var repository = new SqlRepository(new TestDbContextFactory(options));

            var vehicle = new VehicleDto
            {
                Timestamp = new DateTime(2026, 10, 24, 12, 0, 0),
                AxleCount = 5,
                Speed = 70,
                TotalWeight = 23331,
                VehicleClass = 12,
                MaxAllowedWeightForClass = 32000,
                MaxAllowedSpeedForClass = 100,
                TotalOverWeight = 2996,
                CrimeCodes = "2020",
                PlateNumber = "۱۲ ایران ۳۴۵ ب ۶۷"
            };

            await repository.SaveAsync(
                vehicle,
                imagePath: string.Empty,
                plate: new PlateResultDto { PlateNumber = vehicle.PlateNumber },
                axleAlpha: 1m,
                weightAlpha: 1.5m);

            await using var context = new AppDbContext(options);
            var saved = await context.Vehicles.SingleAsync();

            Assert.Equal("2020", saved.CrimeCodes);
            Assert.Equal(34996d, saved.TotalWeight);
            Assert.Equal(2996d, saved.TotalOverWeight);
            Assert.Equal(12, saved.VehicleClass);
            Assert.Equal(32000d, saved.MaxAllowedWeightForClass);
        }

        [Fact]
        public async Task SaveAsync_NoViolationStoresNullCrimeCodes()
        {
            var options = CreateOptions();
            var repository = new SqlRepository(new TestDbContextFactory(options));

            var vehicle = new VehicleDto
            {
                AxleCount = 2,
                Speed = 80,
                TotalWeight = 2626,
                VehicleClass = 3,
                MaxAllowedWeightForClass = 6000,
                MaxAllowedSpeedForClass = 110,
                CrimeCodes = null
            };

            await repository.SaveAsync(vehicle, string.Empty, new PlateResultDto(), 1m, 1.5m);

            await using var context = new AppDbContext(options);
            var saved = await context.Vehicles.SingleAsync();

            Assert.Null(saved.CrimeCodes);
            Assert.Equal(3939d, saved.TotalWeight);
        }

        [Fact]
        public async Task UpdatingOtherColumns_DoesNotResetStoredCrimeCodes()
        {
            var options = CreateOptions();
            var repository = new SqlRepository(new TestDbContextFactory(options));

            await repository.SaveAsync(
                new VehicleDto
                {
                    AxleCount = 2,
                    Speed = 120,
                    TotalWeight = 10170,
                    VehicleClass = 4,
                    MaxAllowedWeightForClass = 10000,
                    MaxAllowedSpeedForClass = 100,
                    TotalOverWeight = 5255,
                    CrimeCodes = "2056,2020"
                },
                string.Empty,
                new PlateResultDto(),
                1m,
                1.5m);

            await using (var context = new AppDbContext(options))
            {
                var saved = await context.Vehicles.SingleAsync();
                saved.PlateNumber = "changed-after-insert";
                await context.SaveChangesAsync();
            }

            await using (var context = new AppDbContext(options))
            {
                var reloaded = await context.Vehicles.SingleAsync();
                Assert.Equal("changed-after-insert", reloaded.PlateNumber);
                Assert.Equal("2056,2020", reloaded.CrimeCodes);
            }
        }

        private static DbContextOptions<AppDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"CrimeCodes_{Guid.NewGuid():N}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        private sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>
        {
            private readonly DbContextOptions<AppDbContext> _options;

            public TestDbContextFactory(DbContextOptions<AppDbContext> options) => _options = options;

            public AppDbContext CreateDbContext() => new(_options);

            public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
                Task.FromResult(CreateDbContext());
        }
    }
}
