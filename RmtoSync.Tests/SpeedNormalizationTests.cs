using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Services;
using Xunit;

namespace RmtoSync.Tests;

/// <summary>
/// رگرسیون خطاهای ITS 116/124: مقدار صفر یعنی «سرعت ثبت نشده» و با speedType = 1/2/3
/// نامعتبر است. قبلاً رکوردهایی که VehicleSpeed = 0 داشتند با پیام
/// «Instant speed requires VehicleSpeed &gt; 0» سه بار شکست می‌خوردند و رها می‌شدند.
/// حالا هر سرعت صفر/NULL با DefaultSpeed جایگزین می‌شود و speedType با مقادیر
/// واقعاً ارسالی هم‌خوان می‌ماند.
/// </summary>
public sealed class SpeedNormalizationTests
{
    // ── ANPR-only station: both speeds are 0 in the DB ────────────────────────────────

    [Fact]
    public void AnprStation_replaces_zero_speeds_with_default_speed()
    {
        var options = new RahdariOptions { AnprOnlyStation = true, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 0, averageSpeed: 0);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(60, payload.VehicleSpeed);
        Assert.Equal(60, payload.AverageSpeed);
        Assert.Equal(TtoFieldValues.SpeedType.Instant, payload.SpeedType);
    }

    [Fact]
    public void AnprStation_zero_speeds_pass_pre_send_validation()
    {
        var options = new RahdariOptions { AnprOnlyStation = true, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 0, averageSpeed: 0);

        var payload = TtoPayloadFactory.Create(photo, options);

        // قبل از اصلاح، این فراخوانی با «Instant speed requires VehicleSpeed > 0 (ITS 116)» می‌افتاد.
        TtoPreSendValidator.Validate(payload, colorImage: null, plateImage: null, requireImages: false);
    }

    [Fact]
    public void AnprStation_only_fills_the_missing_speed_value()
    {
        var options = new RahdariOptions { AnprOnlyStation = true, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 0, averageSpeed: 129);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(60, payload.VehicleSpeed);
        Assert.Equal(129, payload.AverageSpeed);
        Assert.Equal(TtoFieldValues.SpeedType.Instant, payload.SpeedType);
    }

    // ── Full station: speedType must match the values actually sent ────────────────────

    [Fact]
    public void FullStation_zero_speeds_and_no_speed_type_become_instant_default()
    {
        var options = new RahdariOptions { AnprOnlyStation = false, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 0, averageSpeed: 0);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(60, payload.VehicleSpeed);
        Assert.Equal(60, payload.AverageSpeed);
        Assert.Equal(TtoFieldValues.SpeedType.Instant, payload.SpeedType);
        TtoPreSendValidator.Validate(payload, null, null, requireImages: false);
    }

    [Fact]
    public void FullStation_average_type_without_previous_device_falls_back_to_instant()
    {
        var options = new RahdariOptions { AnprOnlyStation = false, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 0, averageSpeed: 120, speedType: 2, previousDeviceCode: 0);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(TtoFieldValues.SpeedType.Instant, payload.SpeedType);
        Assert.Equal(60, payload.VehicleSpeed);
        Assert.Equal(120, payload.AverageSpeed);
        TtoPreSendValidator.Validate(payload, null, null, requireImages: false);
    }

    [Fact]
    public void FullStation_average_type_with_previous_device_is_kept()
    {
        var options = new RahdariOptions { AnprOnlyStation = false, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 0, averageSpeed: 120, speedType: 2, previousDeviceCode: 555);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(TtoFieldValues.SpeedType.Average, payload.SpeedType);
        Assert.Equal(60, payload.VehicleSpeed);
        Assert.Equal(120, payload.AverageSpeed);
        TtoPreSendValidator.Validate(payload, null, null, requireImages: false);
    }

    [Fact]
    public void FullStation_measured_instant_and_average_are_preserved()
    {
        var options = new RahdariOptions { AnprOnlyStation = false, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 90, averageSpeed: 129, speedType: 3, previousDeviceCode: 555);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(TtoFieldValues.SpeedType.InstantAndAverage, payload.SpeedType);
        Assert.Equal(90, payload.VehicleSpeed);
        Assert.Equal(129, payload.AverageSpeed);
    }

    [Fact]
    public void FullStation_measured_instant_speed_is_not_replaced_when_average_is_missing()
    {
        var options = new RahdariOptions { AnprOnlyStation = false, DefaultSpeed = 60 };
        var photo = Photo(vehicleSpeed: 90, averageSpeed: 0, speedType: 3, previousDeviceCode: 555);

        var payload = TtoPayloadFactory.Create(photo, options);

        // میانگین واقعی نیست ⇒ نوع ۱؛ سرعت واقعی ۹۰ باید حفظ شود (قبلاً با fakeSpeed جایگزین می‌شد).
        Assert.Equal(TtoFieldValues.SpeedType.Instant, payload.SpeedType);
        Assert.Equal(90, payload.VehicleSpeed);
        Assert.Equal(60, payload.AverageSpeed);
        TtoPreSendValidator.Validate(payload, null, null, requireImages: false);
    }

    // ── Configuration fallback ────────────────────────────────────────────────────────

    [Fact]
    public void Default_speed_falls_back_to_legacy_key_when_not_configured()
    {
        var options = new RahdariOptions { DefaultSpeed = 0, FakeInstantSpeedKmh = 45 };
        var photo = Photo(vehicleSpeed: 0, averageSpeed: 0);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(45, payload.VehicleSpeed);
        Assert.Equal(45, payload.AverageSpeed);
    }

    [Fact]
    public void Default_speed_is_never_zero()
    {
        var options = new RahdariOptions { DefaultSpeed = 0, FakeInstantSpeedKmh = 0 };
        var photo = Photo(vehicleSpeed: null, averageSpeed: null);

        var payload = TtoPayloadFactory.Create(photo, options);

        Assert.Equal(60, payload.VehicleSpeed);
        Assert.Equal(60, payload.AverageSpeed);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────────

    private static CameraPhotoRecord Photo(
        int? vehicleSpeed,
        int? averageSpeed,
        int? speedType = null,
        long? previousDeviceCode = null) => new()
    {
        PhotoId = 99410,
        LineCode = "L1",
        LineId = 1,
        ImportedAt = DateTime.Now.AddMinutes(-10),
        CapturedAt = DateTime.Now.AddHours(-1),
        PlateP1 = "18",
        PlateP2 = "س",
        PlateP3 = "244",
        PlateP4 = "11",
        VehicleSpeed = vehicleSpeed,
        AverageSpeed = averageSpeed,
        SpeedType = speedType,
        PreviousDeviceCode = previousDeviceCode,
        TotalWeight = 0
    };
}
