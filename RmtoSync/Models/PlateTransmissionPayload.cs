namespace RmtoSync.Models;

/// <summary>Plate fields for Rahdari TTOInfo per ITS web service guide (chapter 2-1).</summary>
public sealed class PlateTransmissionPayload
{
    public long VehiclePlate { get; init; }
    public string? RfidNumber { get; init; }
    public long Lpf { get; init; } = 1;
    public long Slpf { get; init; } = 1;
    public bool UsesRfidNumber => VehiclePlate == 0 && !string.IsNullOrWhiteSpace(RfidNumber);
}
