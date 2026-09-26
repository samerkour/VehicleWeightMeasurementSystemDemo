using System;
using System.Linq;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Domain.Services;
using Xunit;

namespace VehicleWeightMeasurementSystemDemo.Tests
{
    /// <summary>
    /// تست‌های سرویس دامنهٔ اعمال قوانین کلاس‌محور خودرو.
    /// همهٔ مقادیر از appsettings-bound config می‌آیند؛ هیچ عدد جادویی در سرویس وجود ندارد.
    /// </summary>
    public class VehicleComplianceServiceTests
    {
        private static VehicleClassificationSettings CreateSettings(
            double weightThresholdKg = 44000,
            int lightKmh = 110,
            int heavyKmh = 100,
            long speedCrime = 2056,
            long weightCrime = 2020)
        {
            return new VehicleClassificationSettings
            {
                WeightViolationThresholdKg = weightThresholdKg,
                LightVehicleSpeedViolationThresholdKmh = lightKmh,
                HeavyVehicleSpeedViolationThresholdKmh = heavyKmh,
                CrimeCodeSpeedViolation = speedCrime,
                CrimeCodeWeightViolation = weightCrime,
                MaxAllowedWeightByClass = new()
                {
                    [1] = 2200,   // سواری
                    [2] = 3500,   // وانت
                    [3] = 6000,   // کامیون ۲ محور سبک
                    [4] = 10000,  // کامیون ۲ محور سنگین
                    [5] = 15000,  // اتوبوس ۲ محور
                    [6] = 23000,  // اتوبوس ۳ محور
                    [7] = 18000,  // کامیون ۳ محور
                    [8] = 23000,  // تریلر ۳ محور
                    [9] = 24000,  // کامیون ۴ محور
                    [10] = 27000, // تریلر ۴ محور ۱۲ چرخ
                    [11] = 30000, // تریلر ۴ محور ۱۴ چرخ
                    [12] = 32000, // تریلر ۵ محور ۱۲ چرخ
                    [13] = 38000, // تریلر ۵ محور ۱۸ چرخ
                    [14] = 40000  // تریلر ۶ محور
                }
            };
        }

        // ─── 1) Weight calculation per class ──────────────────────────────────────

        [Theory]
        [InlineData(1, 2200)]
        [InlineData(2, 3500)]
        [InlineData(3, 6000)]
        [InlineData(4, 10000)]
        [InlineData(14, 40000)]
        public void GetMaxAllowedWeightForClass_UsesPerClassOverride(int vehicleClass, double expectedKg)
        {
            var service = new VehicleComplianceService(CreateSettings());

            var actual = service.GetMaxAllowedWeightForClass(vehicleClass);

            Assert.Equal(expectedKg, actual);
        }

        [Fact]
        public void GetMaxAllowedWeightForClass_FallsBackToWeightViolationThresholdKg_WhenClassMissing()
        {
            // کلاس در MaxAllowedWeightByClass نیست → WeightViolationThresholdKg (منبع تک‌تایی)
            var settings = CreateSettings();
            settings.MaxAllowedWeightByClass.Remove(7);
            var service = new VehicleComplianceService(settings);

            var actual = service.GetMaxAllowedWeightForClass(7);

            Assert.Equal(44000, actual);
        }

        [Fact]
        public void GetMaxAllowedWeightForClass_UsesEmptyDictionaryFallback_WhenNoPerClassConfig()
        {
            var settings = CreateSettings();
            settings.MaxAllowedWeightByClass.Clear();
            var service = new VehicleComplianceService(settings);

            Assert.Equal(44000, service.GetMaxAllowedWeightForClass(1));
            Assert.Equal(44000, service.GetMaxAllowedWeightForClass(14));
        }

        // ─── 2) Speed rules (light vs heavy) ─────────────────────────────────────

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GetMaxAllowedSpeedForClass_LightClasses_UseLightThreshold(int vehicleClass)
        {
            var service = new VehicleComplianceService(CreateSettings(lightKmh: 110, heavyKmh: 100));

            Assert.Equal(110, service.GetMaxAllowedSpeedForClass(vehicleClass));
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(14)]
        public void GetMaxAllowedSpeedForClass_HeavyClasses_UseHeavyThreshold(int vehicleClass)
        {
            var service = new VehicleComplianceService(CreateSettings(lightKmh: 110, heavyKmh: 100));

            Assert.Equal(100, service.GetMaxAllowedSpeedForClass(vehicleClass));
        }

        [Fact]
        public void GetMaxAllowedSpeedForClass_UnknownClass_TreatedAsLight()
        {
            var service = new VehicleComplianceService(CreateSettings(lightKmh: 110, heavyKmh: 100));

            // کلاس نامعتبر/ناشناخته → مثل سبک (آستانهٔ ملایم‌تر) رفتار می‌شود
            Assert.Equal(110, service.GetMaxAllowedSpeedForClass(0));
        }

        // ─── 3) Crime code selection ─────────────────────────────────────────────

        [Fact]
        public void Assess_NoViolations_ReturnsEmptyCrimeCodesAndCompliant()
        {
            var service = new VehicleComplianceService(CreateSettings());

            // کلاس ۱ (سواری): maxWeight=2200, maxSpeed=110
            var result = service.Assess(1, speedKmh: 80, totalWeightKg: 1500);

            Assert.True(result.IsCompliant);
            Assert.Equal(0, result.TotalOverWeight);
            Assert.Empty(result.CrimeCodes);
        }

        [Fact]
        public void Assess_SpeedViolation_ReturnsSpeedCrimeCode()
        {
            var service = new VehicleComplianceService(CreateSettings());

            // سرعت ۱۲۰ > حد ۱۱۰ کلاس ۱ → فقط کد تخلف سرعت
            var result = service.Assess(1, speedKmh: 120, totalWeightKg: 1500);

            Assert.False(result.IsCompliant);
            Assert.True(result.SpeedViolation);
            Assert.False(result.WeightViolation);
            Assert.Equal(new long[] { 2056 }, result.CrimeCodes);
        }

        [Fact]
        public void Assess_WeightViolation_ReturnsWeightCrimeCode()
        {
            var service = new VehicleComplianceService(CreateSettings());

            // وزن ۳۰۰۰ > حد ۲۲۰۰ کلاس ۱ → فقط کد تخلف وزن
            var result = service.Assess(1, speedKmh: 80, totalWeightKg: 3000);

            Assert.False(result.IsCompliant);
            Assert.False(result.SpeedViolation);
            Assert.True(result.WeightViolation);
            Assert.Equal(800, result.TotalOverWeight); // 3000 - 2200
            Assert.Equal(new long[] { 2020 }, result.CrimeCodes);
        }

        [Fact]
        public void Assess_BothViolations_ReturnsBothCrimeCodes()
        {
            var service = new VehicleComplianceService(CreateSettings());

            // سرعت ۱۲۰ > ۱۱۰ و وزن ۳۰۰۰ > ۲۲۰۰ (کلاس ۱) → هر دو کد
            var result = service.Assess(1, speedKmh: 120, totalWeightKg: 3000);

            Assert.False(result.IsCompliant);
            Assert.True(result.SpeedViolation);
            Assert.True(result.WeightViolation);
            Assert.Equal(new long[] { 2056, 2020 }, result.CrimeCodes);
        }

        [Fact]
        public void Assess_HeavyClassUsesHeavySpeedLimit()
        {
            var service = new VehicleComplianceService(CreateSettings());

            // کلاس ۱۴ (تریلر۶): maxSpeed=100 → سرعت ۱۰۵ تخلف سرعت است
            var result = service.Assess(14, speedKmh: 105, totalWeightKg: 30000);

            Assert.True(result.SpeedViolation);
            Assert.Equal(new long[] { 2056 }, result.CrimeCodes);
        }

        // ─── 4) Config-driven: crime codes read from settings, not hardcoded ─────

        [Fact]
        public void Assess_UsesConfiguredCrimeCodes()
        {
            var service = new VehicleComplianceService(
                CreateSettings(speedCrime: 9999, weightCrime: 8888));

            var result = service.Assess(1, speedKmh: 120, totalWeightKg: 3000);

            Assert.Equal(new long[] { 9999, 8888 }, result.CrimeCodes);
        }

        [Fact]
        public void Assess_UsesConfiguredSpeedThresholds()
        {
            var service = new VehicleComplianceService(
                CreateSettings(lightKmh: 130, heavyKmh: 120));

            // کلاس ۱: حد ۱۳۰ → سرعت ۱۲۵ تخلف نیست
            Assert.Empty(service.Assess(1, speedKmh: 125, totalWeightKg: 1500).CrimeCodes);
        }
    }
}