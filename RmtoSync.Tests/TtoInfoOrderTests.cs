using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RmtoSync.Configuration;
using RmtoSync.Its;
using RmtoSync.Models;
using RmtoSync.Services;
using Xunit;

namespace RmtoSync.Tests;

/// <summary>
/// رگرسیون «HTTP 500 — فیلد cARCLASS13 در این نوع ارسال یا ترتیب XML مجاز نیست».
/// سرور راهداری ترتیب عناصر TTOInfo را طبق sequence وب‌سرویس اعتبارسنجی می‌کند؛
/// cARCLASS13 باید پیش از cOLORIMAGE بیاید (نمونه ۶-۴ راهنما)، نه پس از LPF/SLPF.
/// این تست‌ها ترتیب واقعی XML ارسالی را از بدنه‌ی درخواست addTTOInfo بررسی می‌کنند.
/// </summary>
public sealed class TtoInfoOrderTests
{
    [Fact]
    public async Task Single_addTtoInfo_emits_every_field_in_the_documented_sequence()
    {
        var handler = new CapturingHandler();
        var client = CreateClient(handler);

        await client.SendSingleAsync(HeavyPayload(), [1, 2, 3], [4, 5, 6], CancellationToken.None);

        var names = TtoInfoElementNames(handler.SingleBody);

        string[] expected =
        [
            "Crimes", "aLLOWED",
            "aXLESEQUIVALENT_W1", "aXLESEQUIVALENT_W2", "aXLESEQUIVALENT_W3", "aXLESEQUIVALENT_W4",
            "aXLESEQUIVALENT_W5", "aXLESEQUIVALENT_W6", "aXLESEQUIVALENT_W7", "aXLESEQUIVALENT_W8",
            "aXLESEQUIVALENT_W_MORETHAN8",
            "aXLESWEIGHT1", "aXLESWEIGHT2", "aXLESWEIGHT3", "aXLESWEIGHT4", "aXLESWEIGHT5",
            "aXLESWEIGHT6", "aXLESWEIGHT7", "aXLESWEIGHT8", "aXLESWEIGHT9",
            "cARCLASS13", "cARCLASS15",
            "cOLORIMAGE", "cOMPANYCODE", "dEVICECODE",
            "eXCOLORIMAGE1", "eXCOLORIMAGE2",
            "fIRSTTOLASTAXLESLEN", "gAP", "hEADGAP", "hasImage",
            "lENGTHAXLES12", "lENGTHAXLES23", "lENGTHAXLES34", "lENGTHAXLES45",
            "lENGTHAXLES56", "lENGTHAXLES67", "lENGTHAXLES78", "lENGTHAXLESMORETHAN8",
            "lINENUMBER",
            "pASSDATETIME", "pASSINFOID", "pLATEIMAGE",
            "rECEIVEDATETIME", "ReferenceNo", "reserved10", "reserved7", "sYSTEMCODE",
            "speedType",
            "tOTALAXLES", "tOTALOVERWEIGHT", "tOTALOVERWEIGHT_A",
            "tOTALOVERWEIGHT_AX1", "tOTALOVERWEIGHT_AX2", "tOTALOVERWEIGHT_AX3", "tOTALOVERWEIGHT_AX4",
            "tOTALOVERWEIGHT_AX5", "tOTALOVERWEIGHT_AX6", "tOTALOVERWEIGHT_AX7", "tOTALOVERWEIGHT_AX8",
            "tOTALOVERWEIGHT_AX_MORETHAN8",
            "tOTALOVERWEIGHT_B", "tOTALOVERWEIGHT_C",
            "tOTALWEIGHT", "tOTALWEIGHT_A", "tOTALWEIGHT_B", "tOTALWEIGHT_C",
            "vEHICLECLASS", "vEHICLELEN", "vEHICLEPLATE", "vEHICLESPEED", "wRONGDIRECTION",
            "PreviousDeviceCode", "LPF", "SLPF"
        ];

        Assert.Equal(expected, names);
    }

    [Fact]
    public async Task Single_addTtoInfo_places_carclass13_before_colorimage_for_light_traffic()
    {
        // همان شرایط PhotoId=113365: خودرو سبک، وزن ۵۴۰، بدون تخلف، تصویر همراه تردد.
        var handler = new CapturingHandler();
        var client = CreateClient(handler);

        await client.SendSingleAsync(LightPayload(), [1, 2, 3], [4, 5, 6], CancellationToken.None);

        var names = TtoInfoElementNames(handler.SingleBody);

        Assert.True(IndexOf(names, "cARCLASS13") < IndexOf(names, "cOLORIMAGE"),
            "cARCLASS13 must precede cOLORIMAGE (ITS guide sample 6-4).");
        Assert.True(IndexOf(names, "cOLORIMAGE") < IndexOf(names, "cOMPANYCODE"));
        Assert.True(IndexOf(names, "hEADGAP") < IndexOf(names, "hasImage"));
        Assert.True(IndexOf(names, "hasImage") < IndexOf(names, "lINENUMBER"));
        Assert.True(IndexOf(names, "speedType") < IndexOf(names, "tOTALAXLES"));
        Assert.True(IndexOf(names, "tOTALAXLES") < IndexOf(names, "vEHICLECLASS"));
        Assert.True(IndexOf(names, "wRONGDIRECTION") < IndexOf(names, "LPF"));
        Assert.True(IndexOf(names, "LPF") < IndexOf(names, "SLPF"));
        Assert.DoesNotContain("score", names);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────────

    private static int IndexOf(IReadOnlyList<string> names, string name)
    {
        var index = -1;
        for (var i = 0; i < names.Count; i++)
        {
            if (names[i] == name)
            {
                index = i;
                break;
            }
        }

        Assert.True(index >= 0, $"Element {name} was not sent.");
        return index;
    }

    private static IReadOnlyList<string> TtoInfoElementNames(string? body)
    {
        Assert.False(string.IsNullOrWhiteSpace(body), "addTTOInfo request body was not captured.");

        var document = XDocument.Parse(body!);
        var info = document.Descendants().Single(e => e.Name.LocalName == "TTOInfo");
        return info.Elements().Select(e => e.Name.LocalName).ToList();
    }

    private static TtoPayload HeavyPayload() => new()
    {
        ReferenceNo = 113365,
        DeviceCode = 16953126,
        SystemCode = 202,
        CompanyCode = 207,
        PassDateTime = DateTime.Now.AddHours(-2),
        ReceiveDateTime = DateTime.Now.AddHours(-2),
        LineNumber = 1,
        VehicleSpeed = 90,
        AverageSpeed = 129,
        Allowed = TtoFieldValues.Allowed.Permitted,
        VehicleClass = TtoFieldValues.VehicleClass.Heavy,
        WrongDirection = TtoFieldValues.WrongDirection.Correct,
        CarClass13 = 10,
        CarClass15 = 10,
        SpeedType = TtoFieldValues.SpeedType.InstantAndAverage,
        Reserved7 = "607057",
        PreviousDeviceCode = 555,
        PassInfoId = 14087,
        OcrScore = 80,
        TotalAxles = 4,
        VehicleLen = 0,
        TotalWeight = 15386,
        HeadGap = 0,
        Gap = 0,
        FirstToLastAxlesLen = 1248,
        AxleWeights = [4486, 4325, 3446, 3129, 0, 0, 0, 0, 0],
        AxleLengths = [368, 734, 146, 0, 0, 0, 0, 0],
        AxleEquivalentWeights = new long[8],
        AxleEquivalentWeightMoreThan8 = 0,
        TotalOverWeight = 0,
        TotalWeightA = 4486,
        TotalWeightB = 4325,
        TotalWeightC = 6575,
        TotalOverWeightA = 0,
        TotalOverWeightB = 0,
        TotalOverWeightC = 0,
        AxleOverWeights = new long[8],
        AxleOverWeightMoreThan8 = 0
    };

    private static TtoPayload LightPayload() => new()
    {
        ReferenceNo = 113365,
        DeviceCode = 16953126,
        SystemCode = 202,
        CompanyCode = 207,
        PassDateTime = DateTime.Now.AddHours(-2),
        ReceiveDateTime = DateTime.Now.AddHours(-2),
        LineNumber = 1,
        VehicleSpeed = 82,
        AverageSpeed = 82,
        Allowed = TtoFieldValues.Allowed.Permitted,
        VehicleClass = TtoFieldValues.VehicleClass.Light,
        WrongDirection = TtoFieldValues.WrongDirection.Correct,
        CarClass13 = 1,
        SpeedType = TtoFieldValues.SpeedType.Instant,
        Reserved7 = "607057",
        OcrScore = 89,
        TotalAxles = 2,
        TotalWeight = 540,
        AxleWeights = [270, 270, 0, 0, 0, 0, 0, 0, 0],
        AxleLengths = new long[8],
        AxleEquivalentWeights = new long[8],
        AxleOverWeights = new long[8]
    };

    private static RahdariTtoClient CreateClient(HttpMessageHandler handler) =>
        new(
            Options.Create(new RahdariOptions { ServiceUrl = "http://localhost/rahnari" }),
            new SingleClientFactory(handler),
            new RahdariSyncStatus(),
            NullLogger<RahdariTtoClient>.Instance);

    private sealed class SingleClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public SingleClientFactory(HttpMessageHandler handler) => _client = new HttpClient(handler);

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? SingleBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var action = request.Headers.TryGetValues("SOAPAction", out var values)
                ? values.First()
                : string.Empty;

            if (action == "addTTOInfo")
                SingleBody = await request.Content!.ReadAsStringAsync(cancellationToken);

            var body = action switch
            {
                "Login" => """
                           <LoginResponse xmlns="http://tempuri.org/">
                             <LoginResult><Result>TOKEN</Result></LoginResult>
                           </LoginResponse>
                           """,
                "addTTOInfo" => """
                                <addTTOInfoResponse xmlns="http://tempuri.org/">
                                  <addTTOInfoResult>
                                    <ErrorCode>0</ErrorCode>
                                    <ErrorDesc>ok</ErrorDesc>
                                    <Result>123</Result>
                                  </addTTOInfoResult>
                                </addTTOInfoResponse>
                                """,
                _ => throw new InvalidOperationException(action)
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "text/xml"),
            };
        }
    }
}
