namespace RmtoSync.Data;

public sealed class CameraPhotoRecord
{
    public long PhotoId { get; set; }
    public int LineId { get; set; }
    public string LineCode { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime? CapturedAt { get; set; }
    public DateTime ImportedAt { get; set; }
    public string? PlateP1 { get; set; }
    public string? PlateP2 { get; set; }
    public string? PlateP3 { get; set; }
    public string? PlateP4 { get; set; }
    public double? PlateConfidence { get; set; }
    public int? PlateBoxLeft { get; set; }
    public int? PlateBoxTop { get; set; }
    public int? PlateBoxWidth { get; set; }
    public int? PlateBoxHeight { get; set; }
    public int PlateReadStatus { get; set; }
    public string? PlateFileName { get; set; }
    public string? PlateFullPath { get; set; }
    public string? PlateRelativePath { get; set; }

    public bool TerminalSent { get; set; }
    public bool? TerminalTtoRegistered { get; set; }
    public DateTime? TerminalTtoRegisteredAt { get; set; }
    public DateTime? TerminalImageDeadlineAt { get; set; }
    public bool? TerminalImageExpired { get; set; }
    public DateTime? TerminalImageExpiredAt { get; set; }
    public long? TerminalPassInfoId { get; set; }
    public long? TerminalPackId { get; set; }

    public int? VehicleSpeed { get; set; }
    public int? AverageSpeed { get; set; }
    public int? TotalWeight { get; set; }
    public byte? TotalAxles { get; set; }
    public int? AxleWeight1 { get; set; }
    public int? AxleWeight2 { get; set; }
    public int? AxleWeight3 { get; set; }
    public int? AxleWeight4 { get; set; }
    public int? AxleWeight5 { get; set; }
    public int? AxleWeight6 { get; set; }
    public int? AxleWeight7 { get; set; }
    public int? AxleWeight8 { get; set; }
    public int? AxleWeight9 { get; set; }

    public byte? CarClass13 { get; set; }
    public bool? Allowed { get; set; }
    public bool? WrongDirection { get; set; }
    public int? SpeedType { get; set; }
    public byte? VehicleClass { get; set; }
    public string? CrimeCodes { get; set; }
    public decimal? OcrScore { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }
    public int? VehicleLen { get; set; }
    public int? HeadGap { get; set; }
    public int? Gap { get; set; }
    public int? FirstToLastAxlesLen { get; set; }
    public int? LengthAxles12 { get; set; }
    public int? LengthAxles23 { get; set; }
    public int? LengthAxles34 { get; set; }
    public int? LengthAxles45 { get; set; }
    public int? LengthAxles56 { get; set; }
    public int? LengthAxles67 { get; set; }
    public int? LengthAxles78 { get; set; }
    public int? LengthAxlesMoreThan8 { get; set; }
    public int? TotalWeightA { get; set; }
    public int? TotalWeightB { get; set; }
    public int? TotalWeightC { get; set; }
    public int? TotalOverWeight { get; set; }
    public long? PassInfoId { get; set; }
    public long? PreviousDeviceCode { get; set; }

    public DateTime PassDatetime => CapturedAt ?? ImportedAt;

    public int LineNumber =>
        int.TryParse(LineCode?.Replace("Line", "", StringComparison.OrdinalIgnoreCase).Trim(), out var n)
            ? n
            : LineId;

    public string PlateNoCompact =>
        $"{PlateP4?.Trim()}ایران{PlateP3?.Trim()}{PlateP2?.Trim()}{PlateP1?.Trim()}";

    public bool HasPlate =>
        !string.IsNullOrWhiteSpace(PlateP1) &&
        !string.IsNullOrWhiteSpace(PlateP2) &&
        !string.IsNullOrWhiteSpace(PlateP3) &&
        !string.IsNullOrWhiteSpace(PlateP4);
}
