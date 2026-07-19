using System.Globalization;
using System.Xml.Linq;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;

namespace RmtoSync.Services;

internal static class RahdariSoapNamespaces
{
    public static readonly XNamespace SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
    public static readonly XNamespace Tns = "http://tempuri.org/";
    public static readonly XNamespace Ttow = "http://schemas.datacontract.org/2004/07/TTOWS.Library.Models";
    public static readonly XNamespace Arr = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
}

internal enum TtoInfoEnvelopeKind
{
    /// <summary>addTTOInfo — wrapper is tem:TTOInfo per ITS guide chapter 5 samples.</summary>
    SingleAddTtoInfo,

    /// <summary>addTTOInfoBatch2 items — wrapper is ttow:ttoInfo per WSDL schema.</summary>
    BatchItem
}

internal static class TtoInfoBuilder
{
    public static XElement BuildTtoInfo(
        TtoPayload payload,
        bool includeImages,
        byte[]? colorImage,
        byte[]? plateImage,
        bool includeExtraColorImages = false,
        TtoInfoEnvelopeKind envelope = TtoInfoEnvelopeKind.SingleAddTtoInfo)
    {
        if (envelope == TtoInfoEnvelopeKind.BatchItem)
            return BuildBatchTtoInfo(payload);

        var info = new XElement(RahdariSoapNamespaces.Tns + "TTOInfo");

        // Field order aligned with ITS guide chapter 5 SOAP samples (addTTOInfo).
        info.Add(BuildCrimes(payload.CrimeCodes));
        info.Add(DecimalField("aLLOWED", payload.Allowed));

        if (includeImages && colorImage is { Length: > 0 })
            info.Add(StringField("cOLORIMAGE", Convert.ToBase64String(colorImage)));

        info.Add(DecimalField("cOMPANYCODE", payload.CompanyCode));
        info.Add(DecimalField("dEVICECODE", payload.DeviceCode));

        if (includeExtraColorImages && includeImages)
            info.Add(StringField("eXCOLORIMAGE2", string.Empty));

        info.Add(BoolField("hasImage", includeImages || payload.HasImage));
        info.Add(DecimalField("lINENUMBER", payload.LineNumber));
        info.Add(StringField("pASSDATETIME", FormatDateTime(payload.PassDateTime)));

        if (includeImages && plateImage is { Length: > 0 })
            info.Add(StringField("pLATEIMAGE", Convert.ToBase64String(plateImage)));

        info.Add(StringField("rECEIVEDATETIME", FormatReceiveDateTime(payload.ReceiveDateTime)));

        if (payload.Plate.UsesRfidNumber)
            info.Add(StringField("rFIDNUMBER", payload.Plate.RfidNumber!));

        info.Add(DecimalField("ReferenceNo", payload.ReferenceNo));

        if (payload.SpeedType is TtoFieldValues.SpeedType.Average or TtoFieldValues.SpeedType.InstantAndAverage)
            info.Add(StringField("reserved10", payload.AverageSpeed.ToString(CultureInfo.InvariantCulture)));

        info.Add(StringField("reserved7", payload.Reserved7));
        info.Add(DecimalField("sYSTEMCODE", payload.SystemCode));
        info.Add(DecimalField("speedType", payload.SpeedType));
        info.Add(DecimalField("vEHICLECLASS", payload.VehicleClass));
        info.Add(DecimalField("vEHICLELEN", payload.VehicleLen));
        info.Add(DecimalField("vEHICLEPLATE", payload.Plate.VehiclePlate));
        info.Add(DecimalField("vEHICLESPEED", payload.VehicleSpeed));
        info.Add(DecimalField("wRONGDIRECTION", payload.WrongDirection));
        info.Add(DecimalField("LPF", payload.Plate.Lpf));
        info.Add(DecimalField("SLPF", payload.Plate.Slpf));

        if (payload.CarClass13 != 0)
            info.Add(DecimalField("cARCLASS13", payload.CarClass13));

        if (payload.OcrScore.HasValue)
            info.Add(DecimalField("score", payload.OcrScore.Value));

        if (payload.Longitude.HasValue)
            info.Add(DecimalField("lONGITUDE", payload.Longitude.Value));

        if (payload.Latitude.HasValue)
            info.Add(DecimalField("lATITUDE", payload.Latitude.Value));

        if (payload.PassInfoId.HasValue)
            info.Add(DecimalField("pASSINFOID", payload.PassInfoId.Value));

        if (payload.CarClass15.HasValue)
            info.Add(DecimalField("cARCLASS15", payload.CarClass15.Value));

        AppendWimFields(info, payload);

        if (includeExtraColorImages && includeImages && colorImage is { Length: > 0 })
            info.Add(StringField("eXCOLORIMAGE1", Convert.ToBase64String(colorImage)));

        if (payload.SpeedType is TtoFieldValues.SpeedType.Average or TtoFieldValues.SpeedType.InstantAndAverage)
            info.Add(DecimalField("PreviousDeviceCode", payload.PreviousDeviceCode));

        return info;
    }

    /// <summary>
    /// ITS guide v4 chapter 5 — addTTOInfoBatch2 light traffic (hasImage=false).
    /// No cARCLASS13/score/WIM fields; element order matches WSDL ttow:ttoInfo sequence.
    /// </summary>
    private static XElement BuildBatchTtoInfo(TtoPayload payload)
    {
        var info = new XElement(RahdariSoapNamespaces.Ttow + "ttoInfo");

        info.Add(BuildCrimes(payload.CrimeCodes));
        info.Add(DecimalField("aLLOWED", payload.Allowed));
        info.Add(DecimalField("cOMPANYCODE", payload.CompanyCode));
        info.Add(DecimalField("dEVICECODE", payload.DeviceCode));
        info.Add(BoolField("hasImage", false));
        info.Add(DecimalField("lINENUMBER", payload.LineNumber));
        info.Add(StringField("pASSDATETIME", FormatDateTime(payload.PassDateTime)));
        // ITS guide ch.6 samples: batch uses ISO dateTime with 'T' (not space + timezone).
        info.Add(StringField("rECEIVEDATETIME", FormatDateTime(payload.ReceiveDateTime)));

        if (payload.Plate.UsesRfidNumber)
            info.Add(StringField("rFIDNUMBER", payload.Plate.RfidNumber!));

        info.Add(DecimalField("ReferenceNo", payload.ReferenceNo));

        if (payload.SpeedType is TtoFieldValues.SpeedType.Average or TtoFieldValues.SpeedType.InstantAndAverage)
            info.Add(StringField("reserved10", payload.AverageSpeed.ToString(CultureInfo.InvariantCulture)));

        info.Add(StringField("reserved7", payload.Reserved7));
        info.Add(DecimalField("sYSTEMCODE", payload.SystemCode));
        info.Add(DecimalField("speedType", payload.SpeedType));
        info.Add(DecimalField("vEHICLECLASS", payload.VehicleClass));

        if (payload.VehicleLen != 0)
            info.Add(DecimalField("vEHICLELEN", payload.VehicleLen));

        info.Add(DecimalField("vEHICLEPLATE", payload.Plate.VehiclePlate));
        info.Add(DecimalField("vEHICLESPEED", payload.VehicleSpeed));
        info.Add(DecimalField("wRONGDIRECTION", payload.WrongDirection));

        if (payload.SpeedType is TtoFieldValues.SpeedType.Average or TtoFieldValues.SpeedType.InstantAndAverage)
            info.Add(DecimalField("PreviousDeviceCode", payload.PreviousDeviceCode));

        info.Add(StringField("LPF", payload.Plate.Lpf.ToString(CultureInfo.InvariantCulture)));
        info.Add(StringField("SLPF", payload.Plate.Slpf.ToString(CultureInfo.InvariantCulture)));

        return info;
    }

    public static XElement BuildImageInfo(
        TtoPayload payload,
        byte[] colorImage,
        byte[] plateImage)
    {
        // WSDL (Documents/wsdl.xml) — AddImage expects element name `image_info`
        // and ImageInfo fields with exact casing/order:
        // ALLOWED, COLORIMAGE, DeviceId, PASSDATETIME, PLATEIMAGE, RefNo, VehicleClass.
        return new XElement(RahdariSoapNamespaces.Tns + "image_info",
            DecimalField("ALLOWED", payload.Allowed),
            StringField("COLORIMAGE", Convert.ToBase64String(colorImage)),
            DecimalField("DeviceId", payload.DeviceCode),
            StringField("PASSDATETIME", FormatDateTime(payload.PassDateTime)),
            StringField("PLATEIMAGE", Convert.ToBase64String(plateImage)),
            DecimalField("RefNo", payload.ReferenceNo),
            DecimalField("VehicleClass", payload.VehicleClass));
    }

    private static void AppendWimFields(XElement info, TtoPayload payload)
    {
        if (!payload.IsHeavy && payload.TotalWeight <= 0)
            return;

        info.Add(DecimalField("tOTALAXLES", payload.TotalAxles));
        info.Add(DecimalField("tOTALWEIGHT", payload.TotalWeight));
        info.Add(DecimalField("hEADGAP", payload.HeadGap));
        info.Add(DecimalField("gAP", payload.Gap));
        info.Add(DecimalField("fIRSTTOLASTAXLESLEN", payload.FirstToLastAxlesLen));

        AddAxleArray(info, "aXLESWEIGHT", payload.AxleWeights, 9);
        AppendLengthAxles(info, payload.AxleLengths);
        AddAxleArray(info, "aXLESEQUIVALENT_W", payload.AxleEquivalentWeights, 8);
        info.Add(DecimalField("aXLESEQUIVALENT_W_MORETHAN8", payload.AxleEquivalentWeightMoreThan8));

        info.Add(DecimalField("tOTALOVERWEIGHT", payload.TotalOverWeight));
        info.Add(DecimalField("tOTALWEIGHT_A", payload.TotalWeightA));
        info.Add(DecimalField("tOTALWEIGHT_B", payload.TotalWeightB));
        info.Add(DecimalField("tOTALWEIGHT_C", payload.TotalWeightC));
        info.Add(DecimalField("tOTALOVERWEIGHT_A", payload.TotalOverWeightA));
        info.Add(DecimalField("tOTALOVERWEIGHT_B", payload.TotalOverWeightB));
        info.Add(DecimalField("tOTALOVERWEIGHT_C", payload.TotalOverWeightC));

        AddOverweightAxles(info, payload.AxleOverWeights);
        info.Add(DecimalField("tOTALOVERWEIGHT_AX_MORETHAN8", payload.AxleOverWeightMoreThan8));
    }

    private static void AppendLengthAxles(XElement info, long[] lengths)
    {
        var names = new[]
        {
            "lENGTHAXLES12", "lENGTHAXLES23", "lENGTHAXLES34", "lENGTHAXLES45",
            "lENGTHAXLES56", "lENGTHAXLES67", "lENGTHAXLES78", "lENGTHAXLESMORETHAN8"
        };

        for (var i = 0; i < names.Length; i++)
            info.Add(DecimalField(names[i], lengths[i]));
    }

    private static void AddOverweightAxles(XElement info, long[] values)
    {
        for (var i = 0; i < 8; i++)
            info.Add(DecimalField($"tOTALOVERWEIGHT_AX{i + 1}", values[i]));
    }

    private static void AddAxleArray(XElement info, string prefix, long[] values, int count)
    {
        for (var i = 0; i < count; i++)
            info.Add(DecimalField($"{prefix}{i + 1}", values[i]));
    }

    private static XElement BuildCrimes(IReadOnlyList<long> crimeCodes)
    {
        var crimes = new XElement(RahdariSoapNamespaces.Ttow + "Crimes");
        foreach (var code in crimeCodes)
            crimes.Add(new XElement(RahdariSoapNamespaces.Arr + "Crime",
                new XElement(RahdariSoapNamespaces.Arr + "crime", code)));
        return crimes;
    }

    private static XElement DecimalField(string name, long value) =>
        new(RahdariSoapNamespaces.Ttow + name, value);

    private static XElement DecimalField(string name, decimal value) =>
        new(RahdariSoapNamespaces.Ttow + name, value);

    private static XElement StringField(string name, string value) =>
        new(RahdariSoapNamespaces.Ttow + name, value);

    private static XElement BoolField(string name, bool value) =>
        new(RahdariSoapNamespaces.Ttow + name, value);

    /// <summary>ITS guide v4 chapter 6 SOAP samples (PassDateTime / AddImage).</summary>
    private static string FormatDateTime(DateTime value) =>
        value.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff", CultureInfo.InvariantCulture);

    /// <summary>addTTOInfo single send — PDF sample 3 allows optional timezone offset.</summary>
    private static string FormatReceiveDateTime(DateTime value)
    {
        var local = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Local)
            : value;

        return local.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffffzzz", CultureInfo.InvariantCulture);
    }
}
