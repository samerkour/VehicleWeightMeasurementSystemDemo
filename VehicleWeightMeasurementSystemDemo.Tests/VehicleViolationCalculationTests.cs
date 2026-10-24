using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Domain.Services;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;
using Xunit;

namespace VehicleWeightMeasurementSystemDemo.Tests
{
    /// <summary>
    /// End-to-end checks for the weight/class/violation workflow: the calibrated
    /// weight must drive classification, limits, overweight and crime codes.
    /// </summary>
    public class VehicleViolationCalculationTests
    {
        private static VehicleClassificationSettings Config() => new();

        // ─── Calibration ──────────────────────────────────────────────────────

        [Fact]
        public void Calibration_AppliesAlphaOnceAndRoundsLikePersistence()
        {
            Assert.Equal(15255d, VehicleWeightCalibration.Apply(10170, 1.5m));
            Assert.Equal(3939d, VehicleWeightCalibration.Apply(2626, 1.5m));
            Assert.Equal(34996d, VehicleWeightCalibration.Apply(23331, 1.5m));
        }

        [Fact]
        public void Calibration_AlphaOneKeepsRawWeight()
        {
            Assert.Equal(6960d, VehicleWeightCalibration.Apply(6960, 1m));
        }

        [Fact]
        public void Calibration_NullWeightStaysNull()
        {
            Assert.Null(VehicleWeightCalibration.Apply(null, 1.5m));
        }

        // ─── Classification must use the calibrated weight ─────────────────────

        [Theory]
        [InlineData(2, 3939d, 3)]   // pickup threshold 3500, light-truck 6000
        [InlineData(2, 6960d, 4)]   // heavier than 6000 -> heavy 2-axle
        [InlineData(3, 19604d, 8)]  // 3-axle threshold 18000
        [InlineData(4, 25194d, 10)] // 4-axle threshold 24000
        [InlineData(5, 34996d, 12)] // 5-axle is always class 12
        [InlineData(6, 44720d, 14)] // 6+ axles is always class 14
        public void Classify_UsesCalibratedWeight(int axles, double calibratedWeight, int expectedClass)
        {
            var vehicle = new VehicleDto { AxleCount = axles, TotalWeight = calibratedWeight };

            var actual = VehicleClassifier.Classify(vehicle, Config(), calibratedWeight);

            Assert.Equal(expectedClass, actual);
        }

        [Fact]
        public void Classify_WithoutOverrideKeepsRawWeightForCompatibility()
        {
            var vehicle = new VehicleDto { AxleCount = 2, TotalWeight = 2626 };

            Assert.Equal(2, VehicleClassifier.Classify(vehicle, Config()));
            Assert.Equal(3, VehicleClassifier.Classify(vehicle, Config(), 3939));
        }

        [Theory]
        [InlineData(2, 1500d, 1)]
        [InlineData(2, 1500.01d, 2)]
        [InlineData(2, 3500d, 2)]
        [InlineData(2, 3500.01d, 3)]
        [InlineData(2, 6000d, 3)]
        [InlineData(2, 6000.01d, 4)]
        [InlineData(3, 18000d, 7)]
        [InlineData(3, 18000.01d, 8)]
        [InlineData(4, 24000d, 9)]
        [InlineData(4, 24000.01d, 10)]
        public void Classify_UsesExactBoundaries(int axles, double weight, int expectedClass)
        {
            var vehicle = new VehicleDto { AxleCount = axles, TotalWeight = weight };

            Assert.Equal(expectedClass, VehicleClassifier.Classify(vehicle, Config(), weight));
        }

        // ─── Whole workflow ────────────────────────────────────────────────────

        [Fact]
        public void Evaluate_WeightViolationUsesCalibratedWeight_NotRawWeight()
        {
            // 23,331 raw × 1.5 = 34,996 kg. 5 axles -> class 12, limit 32,000.
            // Before the fix the assessment used 23,331 and stored no 2020 code.
            var vehicle = new VehicleDto { AxleCount = 5, TotalWeight = 23331, Speed = 70 };
            var service = new VehicleComplianceService(Config());

            var assessment = VehicleViolationEvaluator.Evaluate(vehicle, Config(), service, 1.5m, plateRead: true);

            Assert.Equal(12, vehicle.VehicleClass);
            Assert.Equal(32000d, vehicle.MaxAllowedWeightForClass);
            Assert.Equal(2996d, vehicle.TotalOverWeight);
            Assert.Equal("2020", vehicle.CrimeCodes);
            Assert.Equal(new long[] { 2020 }, assessment.CrimeCodes);
            Assert.False(vehicle.Allowed ?? false);
        }

        [Fact]
        public void Evaluate_BothViolationsProducesBothCodesInDocumentedOrder()
        {
            // 10,170 raw × 1.5 = 15,255 kg, 2 axles -> class 4, limit 10,000 / 100 km/h.
            var vehicle = new VehicleDto { AxleCount = 2, TotalWeight = 10170, Speed = 120 };
            var service = new VehicleComplianceService(Config());

            VehicleViolationEvaluator.Evaluate(vehicle, Config(), service, 1.5m, plateRead: true);

            Assert.Equal(4, vehicle.VehicleClass);
            Assert.Equal(5255d, vehicle.TotalOverWeight);
            Assert.Equal("2056,2020", vehicle.CrimeCodes);
        }

        [Fact]
        public void Evaluate_ReclassifiedVehicleUsesItsNewSpeedLimit()
        {
            // 4,640 raw × 1.5 = 6,960 kg on 2 axles -> class 4 (heavy), 100 km/h.
            // If the raw weight were used, the vehicle would stay class 3 (110 km/h)
            // and a 105 km/h pass would wrongly have no speed code.
            var vehicle = new VehicleDto { AxleCount = 2, TotalWeight = 4640, Speed = 105 };
            var service = new VehicleComplianceService(Config());

            VehicleViolationEvaluator.Evaluate(vehicle, Config(), service, 1.5m, plateRead: true);

            Assert.Equal(4, vehicle.VehicleClass);
            Assert.Equal(100, vehicle.MaxAllowedSpeedForClass);
            Assert.Equal("2056", vehicle.CrimeCodes);
        }

        [Fact]
        public void Evaluate_NoViolationLeavesCrimeCodesNull()
        {
            // 2,626 raw × 1.5 = 3,939 kg on 2 axles -> class 3, limit 6,000 / 110 km/h.
            var vehicle = new VehicleDto { AxleCount = 2, TotalWeight = 2626, Speed = 80 };
            var service = new VehicleComplianceService(Config());

            var assessment = VehicleViolationEvaluator.Evaluate(vehicle, Config(), service, 1.5m, plateRead: true);

            Assert.True(assessment.IsCompliant);
            Assert.Null(vehicle.CrimeCodes);
            Assert.Equal(0d, vehicle.TotalOverWeight);
            Assert.True(vehicle.Allowed);
        }

        [Fact]
        public void Evaluate_UnreadPlateIsNotAllowedEvenWhenCompliant()
        {
            var vehicle = new VehicleDto { AxleCount = 2, TotalWeight = 2626, Speed = 80 };
            var service = new VehicleComplianceService(Config());

            var assessment = VehicleViolationEvaluator.Evaluate(
                vehicle, Config(), service, 1.5m, plateRead: false);

            Assert.True(assessment.IsCompliant);
            Assert.Null(vehicle.CrimeCodes);
            Assert.False(vehicle.Allowed);
        }

        // ─── Thresholds and configuration ─────────────────────────────────────

        [Fact]
        public void Assess_ValuesExactlyAtThresholdsAreNotViolations()
        {
            var service = new VehicleComplianceService(Config());

            Assert.Empty(service.Assess(1, speedKmh: 110, totalWeightKg: 2200).CrimeCodes);
            Assert.Single(service.Assess(1, speedKmh: 110.1, totalWeightKg: 2200).CrimeCodes);
            Assert.Single(service.Assess(1, speedKmh: 110, totalWeightKg: 2200.1).CrimeCodes);
        }

        [Fact]
        public void Assess_ZeroSpeedCodeIsRejectedInsteadOfPersistingZero()
        {
            var settings = Config();
            settings.CrimeCodeSpeedViolation = 0;
            var service = new VehicleComplianceService(settings);

            Assert.Throws<InvalidOperationException>(
                () => service.Assess(1, speedKmh: 120, totalWeightKg: 1500));
        }

        [Fact]
        public void Assess_ZeroWeightCodeIsRejectedInsteadOfPersistingZero()
        {
            var settings = Config();
            settings.CrimeCodeWeightViolation = 0;
            var service = new VehicleComplianceService(settings);

            Assert.Throws<InvalidOperationException>(
                () => service.Assess(1, speedKmh: 80, totalWeightKg: 3000));
        }

        [Fact]
        public void UpdateSettings_AppliesNewLimitsImmediately()
        {
            var service = new VehicleComplianceService(Config());
            Assert.Empty(service.Assess(1, speedKmh: 100, totalWeightKg: 1500).CrimeCodes);

            var updated = Config();
            updated.LightVehicleSpeedViolationThresholdKmh = 90;
            service.UpdateSettings(updated);

            Assert.Equal(new long[] { 2056 }, service.Assess(1, speedKmh: 100, totalWeightKg: 1500).CrimeCodes);
        }

        [Fact]
        public void AppSettings_ConfiguredCrimeCodesAreLoadedAndUsed()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.demo.json", optional: false)
                .Build();

            var settings = config.GetSection("VehicleClassification").Get<VehicleClassificationSettings>();

            Assert.NotNull(settings);
            Assert.Equal(2056, settings!.CrimeCodeSpeedViolation);
            Assert.Equal(2020, settings.CrimeCodeWeightViolation);
            Assert.Equal(110, settings.LightVehicleSpeedViolationThresholdKmh);
            Assert.Equal(100, settings.HeavyVehicleSpeedViolationThresholdKmh);

            var service = new VehicleComplianceService(settings);
            var codes = service.Assess(1, speedKmh: 120, totalWeightKg: 3000).CrimeCodes;
            Assert.Equal("2056,2020", VehicleComplianceService.FormatCrimeCodes(codes));
        }

        [Fact]
        public void MissingConfigurationSection_FallsBackToDocumentedDefaults()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();

            services.Configure<VehicleClassificationSettings>(
                config.GetSection("VehicleClassification"));

            using var provider = services.BuildServiceProvider();
            var settings = provider
                .GetRequiredService<IOptions<VehicleClassificationSettings>>()
                .Value;

            Assert.Equal(2056, settings.CrimeCodeSpeedViolation);
            Assert.Equal(2020, settings.CrimeCodeWeightViolation);
        }

        [Fact]
        public void FormatCrimeCodes_UsesCommaSeparatedStorageFormat()
        {
            Assert.Null(VehicleComplianceService.FormatCrimeCodes(Array.Empty<long>()));
            Assert.Equal("2056", VehicleComplianceService.FormatCrimeCodes(new long[] { 2056 }));
            Assert.Equal("2056,2020", VehicleComplianceService.FormatCrimeCodes(new long[] { 2056, 2020 }));
        }
    }
}
