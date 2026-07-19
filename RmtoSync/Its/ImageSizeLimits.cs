namespace RmtoSync.Its;

/// <summary>Table 3-1 — allowed image payload sizes (KB). ITS guide chapter 2-2.</summary>
public enum ItsImageKind
{
    Main,
    MainAverageSpeed,
    MainWim,
    Plate,
    Infrared
}

public static class ImageSizeLimits
{
    public sealed record Limit(int? MinKb, int MaxKb);

    private static readonly IReadOnlyDictionary<ItsImageKind, Limit> Limits = new Dictionary<ItsImageKind, Limit>
    {
        [ItsImageKind.Main] = new(15, 300),
        [ItsImageKind.MainAverageSpeed] = new(15, 465),
        [ItsImageKind.MainWim] = new(15, 700),
        [ItsImageKind.Plate] = new(1, 50),
        [ItsImageKind.Infrared] = new(null, 40)
    };

    public static Limit Get(ItsImageKind kind) => Limits[kind];

    public static int MinBytes(ItsImageKind kind) => (Get(kind).MinKb ?? 0) * 1024;

    public static int MaxBytes(ItsImageKind kind) => Get(kind).MaxKb * 1024;
}
