using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;
using RmtoSync.Utilities;

namespace RmtoSync.Services;

public static class TtoPayloadFactory
{
    public static TtoPayload Create(CameraPhotoRecord photo, RahdariOptions options)
    {
        var payload = options.AnprOnlyStation
            ? CreateAnprOnly(photo, options)
            : CreateFullStation(photo, options);

        return ApplyOutboundFlags(payload, photo, options);
    }

    private static TtoPayload ApplyOutboundFlags(TtoPayload payload, CameraPhotoRecord photo, RahdariOptions options)
    {
        var batchDeferredImages = options.UseBatchSend &&
                                  options.SendImagesSeparately &&
                                  !payload.RequiresSingleSend;

        payload.HasImage = !batchDeferredImages;
        payload.PassInfoId = photo.TerminalTtoRegistered == true
            ? photo.TerminalPassInfoId ?? photo.PassInfoId
            : null;

        return payload;
    }

    /// <summary>ANPR camera: plate + scene only — permitted light traffic (ITS chapter 4-1).</summary>
    private static TtoPayload CreateAnprOnly(CameraPhotoRecord photo, RahdariOptions options)
    {
        var plate = PlatePayloadEncoder.Encode(photo);
        var carClass13 = photo.CarClass13 is >= 1 and <= 14
            ? photo.CarClass13.Value
            : options.DefaultCarClass13;
        var (passDateTime, receiveDateTime) = NormalizePassAndReceiveDates(photo.PassDatetime, photo.ImportedAt);

        // ── Maps incoming DB fields one-to-one (Vehicles table) ────────────────────
        // VehicleSpeed → Vehicles.Speed, AverageSpeed → Vehicles.AverageSpeed,
        // TotalWeight → Vehicles.TotalWeight. Fall back to stub speed only when missing.
        var fakeSpeed = options.FakeInstantSpeedKmh > 0 ? options.FakeInstantSpeedKmh : 60;

        return new TtoPayload
        {
            ReferenceNo = photo.PhotoId,
            DeviceCode = options.DeviceCode,
            SystemCode = options.SystemCode,
            CompanyCode = options.CompanyCode,
            PassDateTime = passDateTime,
            ReceiveDateTime = receiveDateTime,
            Plate = plate,
            LineNumber = photo.LineNumber,
            VehicleSpeed = photo.VehicleSpeed ?? fakeSpeed,
            AverageSpeed = photo.AverageSpeed ?? 0,
            Allowed = TtoFieldValues.Allowed.Permitted,
            VehicleClass = options.DefaultVehicleClass,
            WrongDirection = ToWrongDirection(photo.WrongDirection),
            CarClass13 = carClass13,
            SpeedType = TtoFieldValues.SpeedType.Instant,
            Reserved7 = options.Reserved7,
            PreviousDeviceCode = 0,
            OcrScore = photo.OcrScore ?? ToScorePercent(photo.PlateConfidence),
            Longitude = photo.Longitude ?? options.StationLongitude,
            Latitude = photo.Latitude ?? options.StationLatitude,
            CrimeCodes = Array.Empty<long>(),
            TotalAxles = 0,
            VehicleLen = 0,
            // TotalWeight از داده ورودی (Vehicles.TotalWeight)
            TotalWeight = photo.TotalWeight ?? 0,
            HeadGap = 0,
            Gap = 0,
            FirstToLastAxlesLen = 0,
            AxleWeights = EmptyAxleWeights,
            AxleLengths = EmptyAxleLengths,
            TotalWeightA = 0,
            TotalWeightB = 0,
            TotalWeightC = 0,
            TotalOverWeight = 0
        };
    }

    private static TtoPayload CreateFullStation(CameraPhotoRecord photo, RahdariOptions options)
    {
        var plate = PlatePayloadEncoder.Encode(photo);
        var (passDate, receiveDate) = NormalizePassAndReceiveDates(photo.PassDatetime, photo.ImportedAt);
        var speed = photo.VehicleSpeed ?? 0;
        var avgSpeed = photo.AverageSpeed ?? 0;
        var weight = photo.TotalWeight ?? 0;
        var speedType = ResolveSpeedType(speed, avgSpeed, photo.SpeedType);
        (speed, avgSpeed, speedType) = NormalizeSpeedFields(speed, avgSpeed, speedType, options);
        var axles = photo.TotalAxles ?? (weight > 0 ? (byte)2 : (byte)0);

        var crimes = ParseCrimeCodes(photo.CrimeCodes);
        var allowed = photo.Allowed is { } isAllowed
            ? (isAllowed ? TtoFieldValues.Allowed.Permitted : TtoFieldValues.Allowed.Violation)
            : ResolveAllowed(speed, weight, crimes, options);
        if (crimes.Count == 0 && options.EnableAutoViolationDetection)
            crimes = BuildAutoCrimes(speed, weight, allowed, options);

        if (allowed == TtoFieldValues.Allowed.Violation && crimes.Count == 0)
            throw new InvalidOperationException($"PhotoId={photo.PhotoId}: violation traffic requires crime code(s)");

        var vehicleClass = photo.VehicleClass.HasValue
            ? (long)photo.VehicleClass.Value
            : ResolveVehicleClass(weight, options);
        var carClass13 = photo.CarClass13 is >= 1 and <= 14
            ? photo.CarClass13.Value
            : options.DefaultCarClass13;

        return new TtoPayload
        {
            ReferenceNo = photo.PhotoId,
            DeviceCode = options.DeviceCode,
            SystemCode = options.SystemCode,
            CompanyCode = options.CompanyCode,
            PassDateTime = passDate,
            ReceiveDateTime = receiveDate,
            Plate = plate,
            LineNumber = photo.LineNumber,
            VehicleSpeed = speed,
            AverageSpeed = avgSpeed,
            Allowed = allowed,
            VehicleClass = vehicleClass,
            WrongDirection = ToWrongDirection(photo.WrongDirection),
            CarClass13 = carClass13,
            CarClass15 = vehicleClass == TtoFieldValues.VehicleClass.Heavy ? carClass13 : null,
            SpeedType = speedType,
            Reserved7 = options.Reserved7,
            PreviousDeviceCode = photo.PreviousDeviceCode ?? 0,
            OcrScore = photo.OcrScore ?? ToScorePercent(photo.PlateConfidence),
            Longitude = photo.Longitude ?? options.StationLongitude,
            Latitude = photo.Latitude ?? options.StationLatitude,
            CrimeCodes = crimes,
            TotalAxles = axles,
            VehicleLen = photo.VehicleLen ?? 0,
            TotalWeight = weight,
            HeadGap = photo.HeadGap ?? 0,
            Gap = photo.Gap ?? 0,
            FirstToLastAxlesLen = photo.FirstToLastAxlesLen ?? 0,
            AxleWeights = BuildAxleWeights(photo),
            AxleLengths = BuildAxleLengths(photo),
            TotalWeightA = photo.TotalWeightA ?? photo.AxleWeight1 ?? 0,
            TotalWeightB = photo.TotalWeightB ?? photo.AxleWeight2 ?? 0,
            TotalWeightC = photo.TotalWeightC ?? SumAxleWeights(photo, 3),
            TotalOverWeight = photo.TotalOverWeight ?? 0
        };
    }

    private static readonly long[] EmptyAxleWeights = new long[9];
    private static readonly long[] EmptyAxleLengths = new long[8];

    /// <summary>ITS requires ReceiveDateTime &gt;= PassDateTime; camera clock may be ahead of import time.</summary>
    private static (DateTime Pass, DateTime Receive) NormalizePassAndReceiveDates(DateTime pass, DateTime receive) =>
        pass > receive ? (pass, pass) : (pass, receive);

    private static long ResolveAllowed(int speed, int weight, IReadOnlyList<long> crimes, RahdariOptions options)
    {
        if (crimes.Count > 0)
            return TtoFieldValues.Allowed.Violation;

        if (!options.EnableAutoViolationDetection)
            return TtoFieldValues.Allowed.Permitted;

        if (speed >= options.SpeedViolationThresholdKmh || weight >= options.WeightViolationThresholdKg)
            return TtoFieldValues.Allowed.Violation;

        return TtoFieldValues.Allowed.Permitted;
    }

    private static long ResolveVehicleClass(int weight, RahdariOptions options) =>
        weight >= options.WeightViolationThresholdKg
            ? TtoFieldValues.VehicleClass.Heavy
            : options.DefaultVehicleClass;

    private static long ResolveSpeedType(int speed, int avgSpeed, int? explicitType)
    {
        if (explicitType is >= 0 and <= 3)
            return explicitType.Value;

        return (speed, avgSpeed) switch
        {
            (> 0, > 0) => TtoFieldValues.SpeedType.InstantAndAverage,
            (> 0, 0) => TtoFieldValues.SpeedType.Instant,
            (0, > 0) => TtoFieldValues.SpeedType.Average,
            _ => TtoFieldValues.SpeedType.None
        };
    }

    /// <summary>
    /// DB may carry stale speedType=2/3 without average speed — ITS returns error 124.
    /// When no real sensor data, fall back to fake instant speed (same as camera-only stub).
    /// </summary>
    private static (int Speed, int AvgSpeed, long SpeedType) NormalizeSpeedFields(
        int speed, int avgSpeed, long speedType, RahdariOptions options)
    {
        if (speedType is TtoFieldValues.SpeedType.Average or TtoFieldValues.SpeedType.InstantAndAverage
            && avgSpeed <= 0)
        {
            var fakeSpeed = options.FakeInstantSpeedKmh > 0 ? options.FakeInstantSpeedKmh : 60;
            return (fakeSpeed, 0, TtoFieldValues.SpeedType.Instant);
        }

        if (speedType == TtoFieldValues.SpeedType.Instant && speed <= 0)
        {
            var fakeSpeed = options.FakeInstantSpeedKmh > 0 ? options.FakeInstantSpeedKmh : 60;
            return (fakeSpeed, avgSpeed, speedType);
        }

        if (speedType == TtoFieldValues.SpeedType.None && speed <= 0 && avgSpeed <= 0)
        {
            var fakeSpeed = options.FakeInstantSpeedKmh > 0 ? options.FakeInstantSpeedKmh : 60;
            return (fakeSpeed, 0, TtoFieldValues.SpeedType.Instant);
        }

        return (speed, avgSpeed, speedType);
    }

    private static IReadOnlyList<long> BuildAutoCrimes(int speed, int weight, long allowed, RahdariOptions options)
    {
        if (allowed != TtoFieldValues.Allowed.Violation)
            return Array.Empty<long>();

        var crimes = new List<long>(2);
        if (speed >= options.SpeedViolationThresholdKmh)
            crimes.Add(options.SpeedViolationCrimeCode);
        if (weight >= options.WeightViolationThresholdKg)
            crimes.Add(options.WeightViolationCrimeCode);
        return crimes;
    }

    private static IReadOnlyList<long> ParseCrimeCodes(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<long>();

        return raw.Split(',', ';', ' ')
            .Select(p => p.Trim())
            .Where(p => long.TryParse(p, out _))
            .Select(long.Parse)
            .Distinct()
            .ToList();
    }

    private static decimal? ToScorePercent(double? confidence) =>
        confidence is > 0 and <= 1 ? Math.Round((decimal)(confidence.Value * 100), 2) : null;

    private static long ToWrongDirection(bool? wrongDirection) =>
        wrongDirection == true
            ? TtoFieldValues.WrongDirection.Opposite
            : TtoFieldValues.WrongDirection.Correct;

    private static long[] BuildAxleWeights(CameraPhotoRecord photo) =>
    [
        photo.AxleWeight1 ?? 0,
        photo.AxleWeight2 ?? 0,
        photo.AxleWeight3 ?? 0,
        photo.AxleWeight4 ?? 0,
        photo.AxleWeight5 ?? 0,
        photo.AxleWeight6 ?? 0,
        photo.AxleWeight7 ?? 0,
        photo.AxleWeight8 ?? 0,
        photo.AxleWeight9 ?? 0
    ];

    private static long[] BuildAxleLengths(CameraPhotoRecord photo) =>
    [
        photo.LengthAxles12 ?? 0,
        photo.LengthAxles23 ?? 0,
        photo.LengthAxles34 ?? 0,
        photo.LengthAxles45 ?? 0,
        photo.LengthAxles56 ?? 0,
        photo.LengthAxles67 ?? 0,
        photo.LengthAxles78 ?? 0,
        photo.LengthAxlesMoreThan8 ?? 0
    ];

    private static long SumAxleWeights(CameraPhotoRecord photo, int fromIndex)
    {
        var weights = BuildAxleWeights(photo);
        long sum = 0;
        for (var i = fromIndex; i < weights.Length; i++)
            sum += weights[i];
        return sum;
    }
}
