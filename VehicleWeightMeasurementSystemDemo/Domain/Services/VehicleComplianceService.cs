using System;
using System.Collections.Generic;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;

namespace VehicleWeightMeasurementSystemDemo.Domain.Services
{
    /// <summary>
    /// پاسخ ارزیابی یک عبور: محدودیت‌های کلاس خودرو + نتایج تخلف (وزن/سرعت) + کدهای تخلف.
    /// کاملاً config-driven و بدون هیچ عدد جادویی.
    /// </summary>
    public sealed class VehicleViolationAssessment
    {
        /// <summary>کلاس خودرو (1..14) که ارزیابی برای آن انجام شده است.</summary>
        public int VehicleClass { get; init; }

        /// <summary>حداکثر وزن مجاز (kg) برای این کلاس؛ از WeightViolationThresholdKg مشتق و قابل‌بازنویسی per-class.</summary>
        public double MaxAllowedWeightForClass { get; init; }

        /// <summary>حداکثر سرعت مجاز (km/h) برای این کلاس؛ بر اساس سبک/سنگین‌بودن کلاس.</summary>
        public int MaxAllowedSpeedForClass { get; init; }

        /// <summary>میزان اضافه‌وزن (kg)؛ صفر در صورت رعایت محدودیت وزن.</summary>
        public double TotalOverWeight { get; init; }

        /// <summary>آیا سرعت از آستانهٔ مجاز کلاس تجاوز کرده است؟</summary>
        public bool SpeedViolation { get; init; }

        /// <summary>آیا وزن از حد مجاز کلاس تجاوز کرده است؟</summary>
        public bool WeightViolation { get; init; }

        /// <summary>کدهای تخلفِ اعمال‌شده (۲۰۵۶ = سرعت، ۲۰۲۰ = وزن) — خالی در صورت عدم تخلف.</summary>
        public IReadOnlyList<long> CrimeCodes { get; init; } = Array.Empty<long>();

        /// <summary>بدون تخلف وزن و سرعت → عبور مجاز فرض می‌شود (بدون درنظرگرفتن خوانش پلاک).</summary>
        public bool IsCompliant => !SpeedViolation && !WeightViolation;
    }

    /// <summary>
    /// سرویس دامنهٔ اعمال قوانین کلاس‌محور خودرو (حداکثر وزن/سرعت مجاز هر کلاس + کدهای تخلف).
    /// مقادیر همگی از <see cref="VehicleClassificationSettings"/> (appsettings.json) می‌آیند.
    /// </summary>
    public interface IVehicleComplianceService
    {
        /// <summary>حداکثر وزن مجاز (kg) برای کلاس داده‌شده.</summary>
        double GetMaxAllowedWeightForClass(int vehicleClass);

        /// <summary>حداکثر سرعت مجاز (km/h) برای کلاس داده‌شده (سبک/سنگین).</summary>
        int GetMaxAllowedSpeedForClass(int vehicleClass);

        /// <summary>بررسی تخلف‌های وزن و سرعت برای یک عبور و ساخت کدهای تخلف متناظر.</summary>
        VehicleViolationAssessment Assess(int? vehicleClass, double? speedKmh, double? totalWeightKg);
    }

    /// <summary>
    /// پیاده‌سازی پیش‌فرض سرویس اعمال قوانین کلاس‌محور.
    /// برای قابلیت توسعهٔ آینده (کلاس‌های جدید یا قوانین متفاوت) فقط همین یک کلاس تغییر می‌کند.
    /// </summary>
    public sealed class VehicleComplianceService : IVehicleComplianceService
    {
        /// <summary>کلاس‌های ۱ تا ۳ خودرو سبک محسوب می‌شوند.</summary>
        public const int LightVehicleClassMax = 3;

        /// <summary>کلاس‌های ۴ تا ۱۴ خودرو سنگین محسوب می‌شوند.</summary>
        public const int HeavyVehicleClassMin = 4;

        private readonly VehicleClassificationSettings _settings;

        public VehicleComplianceService(VehicleClassificationSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        public double GetMaxAllowedWeightForClass(int vehicleClass)
        {
            // 🔥 WeightThreshold تک‌منبعی است: اگر برای کلاس، مقدار per-class پیکربندی شده
            // (MaxAllowedWeightByClass) استفاده می‌شود؛ در غیر این‌صورت WeightViolationThresholdKg.
            // → هم‌ارزِ «سینگل‌سورس + کلاس‌آگاه» بدون هیچ عدد جادویی.
            return _settings.MaxAllowedWeightByClass != null &&
                   _settings.MaxAllowedWeightByClass.TryGetValue(vehicleClass, out var perClass)
                ? perClass
                : _settings.WeightViolationThresholdKg;
        }

        /// <inheritdoc />
        public int GetMaxAllowedSpeedForClass(int vehicleClass)
        {
            // 🔥 کلاس‌های ۱-۳ سبک → LightVehicleSpeedViolationThresholdKmh؛ ۴-۱۴ سنگین →
            // HeavyVehicleSpeedViolationThresholdKmh. کلاس نامعتبر به‌صورت امن سبک فرض می‌شود.
            return vehicleClass <= LightVehicleClassMax
                ? _settings.LightVehicleSpeedViolationThresholdKmh
                : _settings.HeavyVehicleSpeedViolationThresholdKmh;
        }

        /// <inheritdoc />
        public VehicleViolationAssessment Assess(int? vehicleClass, double? speedKmh, double? totalWeightKg)
        {
            var cls = vehicleClass ?? LightVehicleClassMax;
            var speed = speedKmh ?? 0;
            var weight = totalWeightKg ?? 0;

            var maxWeight = GetMaxAllowedWeightForClass(cls);
            var maxSpeed = GetMaxAllowedSpeedForClass(cls);
            var totalOverWeight = Math.Max(0, weight - maxWeight);
            var speedViolation = speed > maxSpeed;
            var weightViolation = totalOverWeight > 0;

            var crimes = new List<long>(2);
            if (speedViolation) crimes.Add(_settings.CrimeCodeSpeedViolation);   // 2056
            if (weightViolation) crimes.Add(_settings.CrimeCodeWeightViolation); // 2020

            return new VehicleViolationAssessment
            {
                VehicleClass = cls,
                MaxAllowedWeightForClass = maxWeight,
                MaxAllowedSpeedForClass = maxSpeed,
                TotalOverWeight = totalOverWeight,
                SpeedViolation = speedViolation,
                WeightViolation = weightViolation,
                CrimeCodes = crimes
            };
        }
    }
}