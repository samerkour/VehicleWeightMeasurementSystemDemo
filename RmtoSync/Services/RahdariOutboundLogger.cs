using System.Xml.Linq;

namespace RmtoSync.Services;

internal static class RahdariOutboundLogger
{
    private static readonly HashSet<string> SensitiveElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "Token",
    };

    private static readonly HashSet<string> ImageElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "COLORIMAGE",
        "PLATEIMAGE",
        "cOLORIMAGE",
        "pLATEIMAGE",
        "eXCOLORIMAGE1",
        "eXCOLORIMAGE2",
    };

    public static string SanitizeEnvelope(XDocument envelope)
    {
        var clone = new XDocument(envelope);
        foreach (var element in clone.Descendants())
        {
            var name = element.Name.LocalName;
            if (SensitiveElements.Contains(name))
            {
                element.Value = "***";
                continue;
            }

            if (ImageElements.Contains(name) && element.Value.Length > 0)
                element.Value = $"[base64 image, ~{EstimateBase64Bytes(element.Value)} bytes]";
        }

        return clone.ToString(SaveOptions.DisableFormatting);
    }

    private static int EstimateBase64Bytes(string base64) =>
        base64.Length > 0 ? base64.Length * 3 / 4 : 0;
}
