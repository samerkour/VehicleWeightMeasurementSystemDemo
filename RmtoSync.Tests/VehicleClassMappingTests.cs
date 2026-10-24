using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Services;
using Xunit;

namespace RmtoSync.Tests;

/// <summary>
/// Regression coverage for the 2026-10-24 «عدم صحت کلاس خودرو» (ITS 112) rejection.
/// The database stores the 14-class ITS taxonomy in Vehicles.VehicleClass/CarClass13,
/// while TTOInfo.vEHICLECLASS only accepts 1=light / 2=heavy. The two code systems
/// must never be interchanged.
/// </summary>
public sealed class VehicleClassMappingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CarClass13_light_classes_map_to_vehicleclass_1(int carClass13)
    {
        var payload = FullStationPayload(carClass13);

        Assert.Equal(TtoFieldValues.VehicleClass.Light, payload.VehicleClass);
        Assert.False(payload.IsHeavy);
        Assert.Null(payload.CarClass15);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    public void CarClass13_heavy_classes_map_to_vehicleclass_2(int carClass13)
    {
        var payload = FullStationPayload(carClass13);

        Assert.Equal(TtoFieldValues.VehicleClass.Heavy, payload.VehicleClass);
        Assert.True(payload.IsHeavy);
        Assert.Equal(carClass13, payload.CarClass13);
        Assert.Equal(carClass13, payload.CarClass15);
    }

    [Fact]
    public void Photo_165001_class10_is_heavy_and_must_not_send_vehicleclass_10()
    {
        // PhotoId=165001: 4 axles, 24,195 kg, CarClass13=10, permitted.
        var payload = FullStationPayload(carClass13: 10, totalWeight: 24195, totalAxles: 4);

        Assert.Equal(2, payload.VehicleClass);
        Assert.Equal(10, payload.CarClass13);
        Assert.Equal(10, payload.CarClass15);
        Assert.True(payload.RequiresSingleSend);
    }

    [Fact]
    public void Photo_164989_class12_is_heavy_and_must_not_send_vehicleclass_12()
    {
        // PhotoId=164989: 5 axles, 17,610 kg, CarClass13=12, permitted.
        var payload = FullStationPayload(carClass13: 12, totalWeight: 17610, totalAxles: 5);

        Assert.Equal(2, payload.VehicleClass);
        Assert.Equal(12, payload.CarClass13);
        Assert.Equal(12, payload.CarClass15);
        Assert.True(payload.RequiresSingleSend);
    }

    [Fact]
    public void Heavy_single_payload_serializes_vehicleclass_2_and_carclass13_15()
    {
        var payload = FullStationPayload(carClass13: 12, totalWeight: 17610, totalAxles: 5);
        var handler = new CapturingHandler();
        var client = CreateClient(handler);

        client.SendSingleAsync(payload, [1, 2, 3], [4, 5, 6], CancellationToken.None)
            .GetAwaiter().GetResult();

        var info = TtoInfo(handler.SingleBody);
        Assert.Equal("2", Value(info, "vEHICLECLASS"));
        Assert.Equal("12", Value(info, "cARCLASS13"));
        Assert.Equal("12", Value(info, "cARCLASS15"));
    }

    [Fact]
    public void Light_batch_payload_serializes_vehicleclass_1_and_no_carclass13()
    {
        var payload = FullStationPayload(carClass13: 3, totalWeight: 4065, totalAxles: 2);
        var handler = new CapturingHandler();
        var client = CreateClient(handler);

        client.SendBatchAsync([payload], CancellationToken.None).GetAwaiter().GetResult();

        var info = TtoInfo(handler.BatchBody);
        Assert.Equal("1", Value(info, "vEHICLECLASS"));
        Assert.Null(Element(info, "cARCLASS13"));
        Assert.Null(Element(info, "cARCLASS15"));
    }

    [Fact]
    public void Full_station_factory_rejects_missing_classification_instead_of_sending_a_guess()
    {
        var photo = Photo(carClass13: null, vehicleClass: null);

        var ex = Assert.Throws<RahdariSendException>(() =>
            TtoPayloadFactory.Create(photo, FullStationOptions()));

        Assert.Equal(ItsErrorCodes.AddTto.InvalidCarClass, ex.ItsErrorCode);
        Assert.Contains("vehicle class", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pre_send_validator_rejects_vehicleclass_outside_1_2_with_its_112()
    {
        var payload = FullStationPayload(carClass13: 12);
        // Simulate a future regression that writes the 14-class value into vEHICLECLASS.
        var broken = new RmtoSync.Models.TtoPayload
        {
            ReferenceNo = payload.ReferenceNo,
            DeviceCode = payload.DeviceCode,
            SystemCode = payload.SystemCode,
            CompanyCode = payload.CompanyCode,
            PassDateTime = payload.PassDateTime,
            ReceiveDateTime = payload.ReceiveDateTime,
            Plate = payload.Plate,
            LineNumber = payload.LineNumber,
            VehicleSpeed = payload.VehicleSpeed,
            AverageSpeed = payload.AverageSpeed,
            Allowed = payload.Allowed,
            VehicleClass = 12,
            WrongDirection = payload.WrongDirection,
            CarClass13 = 12,
            CarClass15 = 12,
            SpeedType = payload.SpeedType,
            Reserved7 = payload.Reserved7,
            TotalAxles = payload.TotalAxles,
            TotalWeight = payload.TotalWeight,
            AxleWeights = payload.AxleWeights,
            AxleLengths = payload.AxleLengths,
        };

        var ex = Assert.Throws<RahdariSendException>(() =>
            TtoPreSendValidator.Validate(broken, null, null, requireImages: false));

        Assert.Equal(ItsErrorCodes.AddTto.InvalidCarClass, ex.ItsErrorCode);
    }

    [Fact]
    public void Class_rejection_112_is_permanent_and_keeps_the_server_code()
    {
        var ex = Assert.Throws<RahdariSendException>(() =>
            RahdariTtoClient.EnsureTtoSuccess(165001, ItsErrorCodes.AddTto.InvalidCarClass));

        Assert.Equal(ItsErrorCodes.AddTto.InvalidCarClass, ex.ItsErrorCode);
        Assert.True(ex.IsPermanent);
        Assert.Contains("112", ex.Message);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────────

    private static RmtoSync.Models.TtoPayload FullStationPayload(
        int carClass13,
        int totalWeight = 17610,
        int totalAxles = 5) =>
        TtoPayloadFactory.Create(
            Photo(carClass13, vehicleClass: carClass13, totalWeight: totalWeight, totalAxles: totalAxles),
            FullStationOptions());

    private static CameraPhotoRecord Photo(
        int? carClass13,
        int? vehicleClass,
        int totalWeight = 17610,
        int totalAxles = 5) => new()
    {
        PhotoId = 165001,
        LineId = 1,
        LineCode = "Line1",
        FullPath = @"C:\images\165001.jpg",
        FileName = "165001.jpg",
        CapturedAt = DateTime.Now.AddHours(-1),
        ImportedAt = DateTime.Now.AddHours(-1),
        PlateP1 = "12",
        PlateP2 = "ا",
        PlateP3 = "345",
        PlateP4 = "67",
        PlateConfidence = 0.95,
        VehicleSpeed = 66,
        AverageSpeed = 66,
        TotalWeight = totalWeight,
        TotalAxles = (byte)totalAxles,
        VehicleClass = vehicleClass is null ? null : (byte)vehicleClass.Value,
        CarClass13 = carClass13 is null ? null : (byte)carClass13.Value,
        Allowed = true,
        SpeedType = 1,
        CrimeCodes = null,
        AxleWeight1 = totalWeight / 2,
        AxleWeight2 = totalWeight - (totalWeight / 2),
    };

    private static RahdariOptions FullStationOptions() => new()
    {
        ServiceUrl = "http://localhost/rahnari",
        AnprOnlyStation = false,
        UseBatchSend = true,
        SendImagesSeparately = true,
        DefaultCarClass13 = 1,
        DefaultVehicleClass = 1,
        WeightViolationThresholdKg = 44000,
        LightVehicleSpeedViolationThresholdKmh = 110,
        HeavyVehicleSpeedViolationThresholdKmh = 100,
        DefaultSpeed = 60,
        ImageAttachWindowHours = 24,
    };

    private static RahdariTtoClient CreateClient(HttpMessageHandler handler) =>
        new(
            Options.Create(FullStationOptions()),
            new SingleClientFactory(handler),
            new RahdariSyncStatus(),
            NullLogger<RahdariTtoClient>.Instance);

    private static XElement TtoInfo(string? body)
    {
        Assert.False(string.IsNullOrWhiteSpace(body), "SOAP request body was not captured.");
        var document = XDocument.Parse(body!);
        return document.Descendants().Single(e =>
            e.Name.LocalName is "TTOInfo" or "ttoInfo");
    }

    private static XElement? Element(XElement info, string name) =>
        info.Elements().SingleOrDefault(e => e.Name.LocalName == name);

    private static string Value(XElement info, string name) =>
        Element(info, name)?.Value ?? throw new Xunit.Sdk.XunitException($"Element {name} was not sent.");

    private sealed class SingleClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public SingleClientFactory(HttpMessageHandler handler) => _client = new HttpClient(handler);

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? SingleBody { get; private set; }
        public string? BatchBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var action = request.Headers.TryGetValues("SOAPAction", out var values)
                ? values.First()
                : string.Empty;
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);

            if (action == "addTTOInfo")
            {
                SingleBody = body;
                return XmlResponse(
                    """
                    <addTTOInfoResponse xmlns="http://tempuri.org/">
                      <addTTOInfoResult>
                        <ErrorCode>0</ErrorCode>
                        <ErrorDesc></ErrorDesc>
                        <SysError>0</SysError>
                        <Result>165001</Result>
                      </addTTOInfoResult>
                    </addTTOInfoResponse>
                    """);
            }

            if (action == "addTTOInfoBatch2")
            {
                BatchBody = body;
                return XmlResponse(
                    """
                    <addTTOInfoBatch2Response xmlns="http://tempuri.org/">
                      <addTTOInfoBatch2Result>
                        <ErrorCode>0</ErrorCode>
                        <IsSuccessfull>true</IsSuccessfull>
                      </addTTOInfoBatch2Result>
                    </addTTOInfoBatch2Response>
                    """);
            }

            throw new InvalidOperationException($"Unexpected SOAPAction: {action}");
        }

        private static HttpResponseMessage XmlResponse(string xml) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, Encoding.UTF8, "text/xml"),
        };
    }
}
