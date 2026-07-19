namespace RmtoSync.Models;

using RmtoSync.Its;

/// <summary>Complete TTOInfo payload aligned with ITS guide v4 chapter 5-1 (TTOInfo).</summary>
public sealed class TtoPayload
{
    public long ReferenceNo { get; init; }
    public long DeviceCode { get; init; }
    public long SystemCode { get; init; }
    public long CompanyCode { get; init; }
    public DateTime PassDateTime { get; init; }
    public DateTime ReceiveDateTime { get; init; }
    public PlateTransmissionPayload Plate { get; init; } = new();
    public int LineNumber { get; init; }
    public long VehicleSpeed { get; init; }
    public long AverageSpeed { get; init; }
    public long Allowed { get; init; }
    public long VehicleClass { get; init; }
    public long WrongDirection { get; init; }
    public int CarClass13 { get; init; }
    public int? CarClass15 { get; init; }
    public long SpeedType { get; init; }
    public string Reserved7 { get; init; } = string.Empty;
    public bool HasImage { get; set; } = true;
    public long PreviousDeviceCode { get; init; }
    public long? PassInfoId { get; set; }
    public decimal? OcrScore { get; init; }
    public decimal? Longitude { get; init; }
    public decimal? Latitude { get; init; }
    public IReadOnlyList<long> CrimeCodes { get; init; } = Array.Empty<long>();

    public bool IsHeavy => VehicleClass == TtoFieldValues.VehicleClass.Heavy;
    public bool IsViolation => Allowed == TtoFieldValues.Allowed.Violation;
    public bool RequiresSingleSend => IsHeavy || IsViolation;

    public long TotalAxles { get; init; }
    public long VehicleLen { get; init; }
    public long TotalWeight { get; init; }
    public long HeadGap { get; init; }
    public long Gap { get; init; }
    public long FirstToLastAxlesLen { get; init; }
    public long[] AxleWeights { get; init; } = new long[9];
    public long[] AxleLengths { get; init; } = new long[8];
    public long[] AxleEquivalentWeights { get; init; } = new long[8];
    public long AxleEquivalentWeightMoreThan8 { get; init; }
    public long TotalOverWeight { get; init; }
    public long TotalWeightA { get; init; }
    public long TotalWeightB { get; init; }
    public long TotalWeightC { get; init; }
    public long TotalOverWeightA { get; init; }
    public long TotalOverWeightB { get; init; }
    public long TotalOverWeightC { get; init; }
    public long[] AxleOverWeights { get; init; } = new long[8];
    public long AxleOverWeightMoreThan8 { get; init; }
}
