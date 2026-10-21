using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using RmtoSync.Configuration;
using RmtoSync.Its;
using RmtoSync.Models;
using Microsoft.Extensions.Options;

namespace RmtoSync.Services;

public sealed class RahdariTtoClient
{
    private readonly RahdariOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RahdariSyncStatus _syncStatus;
    private readonly ILogger<RahdariTtoClient> _logger;
    private readonly SemaphoreSlim _authLock = new(1, 1);
    private string? _token;

    public RahdariTtoClient(
        IOptions<RahdariOptions> options,
        IHttpClientFactory httpClientFactory,
        RahdariSyncStatus syncStatus,
        ILogger<RahdariTtoClient> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _syncStatus = syncStatus;
        _logger = logger;
    }

    public async Task<BatchSendResult> SendBatchAsync(
        IReadOnlyList<TtoPayload> payloads,
        CancellationToken ct)
    {
        if (payloads.Count == 0)
            return new BatchSendResult(Array.Empty<InquiryInfoResult>(), 0, "empty request");

        if (payloads.Count > ServiceLimits.MaxBatchRecords)
            throw new InvalidOperationException($"Batch exceeds ITS limit of {ServiceLimits.MaxBatchRecords} records");

        await EnsureTokenAsync(ct);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var envelope = BuildAddTtoBatch2Envelope(_token!, payloads);
            var responseXml = await PostSoapAsync("addTTOInfoBatch2", envelope, ct);
            var batchResult = ParseBatch2Result(responseXml);

            if (ItsErrorCodes.IsTokenExpired(batchResult.ErrorCode))
            {
                if (attempt == 0)
                {
                    _logger.LogWarning(
                        "Rahdari token expired during batch send (ErrorCode={Code}) — re-login and retry",
                        batchResult.ErrorCode);
                    await ForceLoginAsync(ct);
                    continue;
                }

                throw new RahdariSendException(
                    0,
                    $"Rahdari addTTOInfoBatch2: token expired (code {batchResult.ErrorCode}) even after re-login",
                    batchResult.ErrorCode);
            }

            // A wrapper-level error means the whole batch was rejected. Previously only three
            // codes were recognised, so every other rejection (expired token, invalid device,
            // authorisation problems, …) was returned as "no items" and the caller reported the
            // misleading "Batch response missing InquiryInfo for record" for every record in
            // the batch. Always surface the real code.
            if (batchResult.ErrorCode != 0 && batchResult.Items.Count == 0)
            {
                _logger.LogError(
                    "Rahdari addTTOInfoBatch2 rejected the batch: ErrorCode={Code} ({Desc}). {Summary}",
                    batchResult.ErrorCode,
                    ItsErrorCodes.Describe(batchResult.ErrorCode),
                    batchResult.ResponseSummary);

                _syncStatus.RecordFailure("addTTOInfoBatch2", new RahdariResultExplanation(
                    "خطای batch",
                    ItsErrorCodes.Describe(batchResult.ErrorCode),
                    ErrorCode: batchResult.ErrorCode,
                    Source: "ITS فصل 3-5"));

                throw new RahdariSendException(
                    0,
                    $"Rahdari addTTOInfoBatch2 rejected the batch: {ItsErrorCodes.Describe(batchResult.ErrorCode)} " +
                    $"(code {batchResult.ErrorCode})",
                    batchResult.ErrorCode);
            }

            foreach (var item in batchResult.Items)
            {
                _logger.LogInformation(
                    "Rahdari batch result Ref={Ref} ValidationCode={Code} ({Desc}) PassInfoId={PassInfoId} PackId={PackId}",
                    item.ReferenceNo,
                    item.ValidationCode,
                    ItsErrorCodes.Describe(item.ValidationCode),
                    item.PassInfoId,
                    item.PackId);
            }

            if (batchResult.Items.Count == 0)
            {
                // The call succeeded at the transport level but the body carried no InquiryInfo.
                // This is an *unconfirmed* outcome, never proof that the server rejected the
                // records, so it must not be treated as a definitive failure.
                _logger.LogWarning(
                    "Rahdari addTTOInfoBatch2 returned no InquiryInfo for {Sent} record(s). " +
                    "Outcome is UNCONFIRMED (not a rejection). {Summary} Body={Body}",
                    payloads.Count,
                    batchResult.ResponseSummary,
                    batchResult.RawResponse);
            }
            else
            {
                _logger.LogInformation(
                    "Rahdari batch accepted: sent {Sent}, InquiryInfo returned {Received}. {Summary}",
                    payloads.Count,
                    batchResult.Items.Count,
                    batchResult.ResponseSummary);
            }

            if (batchResult.Items.Count > 0)
            {
                var first = batchResult.Items[0];
                _syncStatus.RecordSuccess(
                    "addTTOInfoBatch2",
                    $"batch {batchResult.Items.Count} رکورد — نمونه Ref={first.ReferenceNo}: {ItsErrorCodes.Describe(first.ValidationCode)}",
                    first.ReferenceNo,
                    first.ValidationCode);
            }

            return new BatchSendResult(batchResult.Items, batchResult.ErrorCode, batchResult.ResponseSummary);
        }

        throw new RahdariSendException(0, "Rahdari addTTOInfoBatch2 failed after retry");
    }

    public async Task<AddImageResult> SendImageAsync(
        TtoPayload payload,
        byte[] colorImage,
        byte[] plateImage,
        CancellationToken ct)
    {
        var envelope = BuildAddImageEnvelope(payload, colorImage, plateImage);
        var responseXml = await PostSoapAsync("AddImage", envelope, ct);
        return ParseAddImageResult(responseXml);
    }

    public async Task<AddTtoResult> SendSingleAsync(
        TtoPayload payload,
        byte[] colorImage,
        byte[] plateImage,
        CancellationToken ct)
    {
        await EnsureTokenAsync(ct);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var envelope = BuildAddTtoEnvelope(_token!, payload, colorImage, plateImage);
            var responseXml = await PostSoapAsync("addTTOInfo", envelope, ct);
            var result = ParseAddTtoResult(responseXml);
            _logger.LogInformation(
                "Rahdari addTTOInfo result Ref={Ref} ErrorCode={Code} ({Desc}) SysError={SysError}",
                payload.ReferenceNo,
                result.ErrorCode,
                ItsErrorCodes.Describe(result.ErrorCode),
                result.SysError);

            if (ItsErrorCodes.IsTokenExpired(result.ErrorCode) && attempt == 0)
            {
                _logger.LogWarning(
                    "Rahdari token expired (code {Code}), re-login",
                    result.ErrorCode);
                await ForceLoginAsync(ct);
                continue;
            }

            return result;
        }

        throw new InvalidOperationException("Rahdari addTTOInfo failed after retry");
    }

    /// <summary>
    /// ارسال <c>addTTOInfo</c> بدون تصویر برای قطعی‌کردن وضعیت رکوردی که پاسخ batch آن
    /// نامعلوم بوده است. این متد مرجع نهایی است: سرور برای همین <c>ReferenceNo</c> یک
    /// <c>ErrorCode</c> قطعی برمی‌گرداند (۰/۱/۲/۹۹ موفق، ۱۰۱ تکراری، بقیه رد).
    /// </summary>
    public async Task<AddTtoResult> SendTtoOnlyAsync(TtoPayload payload, CancellationToken ct)
    {
        await EnsureTokenAsync(ct);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var info = TtoInfoBuilder.BuildTtoInfo(payload, includeImages: false, null, null);
            var envelope = WrapSoapBody(
                new XElement(RahdariSoapNamespaces.Tns + "addTTOInfo",
                    new XElement(RahdariSoapNamespaces.Tns + "Token", _token),
                    info));

            var responseXml = await PostSoapAsync("addTTOInfo", envelope, ct);
            var result = ParseAddTtoResult(responseXml);

            _logger.LogInformation(
                "Rahdari addTTOInfo (confirm) Ref={Ref} ErrorCode={Code} ({Desc}) SysError={SysError}",
                payload.ReferenceNo,
                result.ErrorCode,
                ItsErrorCodes.Describe(result.ErrorCode),
                result.SysError);

            if (ItsErrorCodes.IsTokenExpired(result.ErrorCode) && attempt == 0)
            {
                await ForceLoginAsync(ct);
                continue;
            }

            return result;
        }

        throw new RahdariSendException(payload.ReferenceNo, "Rahdari addTTOInfo (confirm) failed after retry");
    }

    private async Task EnsureTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_token))
            return;

        await _authLock.WaitAsync(ct);
        try
        {
            if (string.IsNullOrEmpty(_token))
                await ForceLoginAsync(ct);
        }
        finally
        {
            _authLock.Release();
        }
    }

    private async Task ForceLoginAsync(CancellationToken ct)
    {
        var envelope = new XDocument(
            new XElement(RahdariSoapNamespaces.SoapEnv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", RahdariSoapNamespaces.SoapEnv),
                new XAttribute(XNamespace.Xmlns + "tns", RahdariSoapNamespaces.Tns),
                new XElement(RahdariSoapNamespaces.SoapEnv + "Body",
                    new XElement(RahdariSoapNamespaces.Tns + "Login",
                        new XElement(RahdariSoapNamespaces.Tns + "UserName", _options.UserName),
                        new XElement(RahdariSoapNamespaces.Tns + "Password", _options.Password)))));

        var responseXml = await PostSoapAsync("Login", envelope, ct);
        _token = ParseLoginToken(responseXml)
            ?? throw new InvalidOperationException("Rahdari login failed");
        _logger.LogInformation("Rahdari login OK");
    }

    private static XDocument BuildAddTtoBatch2Envelope(string token, IReadOnlyList<TtoPayload> payloads)
    {
        var items = payloads.Select(p => TtoInfoBuilder.BuildTtoInfo(
            p,
            includeImages: false,
            null,
            null,
            envelope: TtoInfoEnvelopeKind.BatchItem));
        return WrapSoapBody(
            new XElement(RahdariSoapNamespaces.Tns + "addTTOInfoBatch2",
                new XElement(RahdariSoapNamespaces.Tns + "Token", token),
                new XElement(RahdariSoapNamespaces.Tns + "TTOInfoItems", items)));
    }

    private static XDocument BuildAddTtoEnvelope(string token, TtoPayload payload, byte[] colorImage, byte[] plateImage)
    {
        var info = TtoInfoBuilder.BuildTtoInfo(payload, includeImages: true, colorImage, plateImage, includeExtraColorImages: true);
        return WrapSoapBody(
            new XElement(RahdariSoapNamespaces.Tns + "addTTOInfo",
                new XElement(RahdariSoapNamespaces.Tns + "Token", token),
                info));
    }

    private XDocument BuildAddImageEnvelope(TtoPayload payload, byte[] colorImage, byte[] plateImage)
    {
        var imageInfo = TtoInfoBuilder.BuildImageInfo(payload, colorImage, plateImage);
        return WrapSoapBody(
            new XElement(RahdariSoapNamespaces.Tns + "AddImage",
                new XElement(RahdariSoapNamespaces.Tns + "userName", _options.UserName),
                new XElement(RahdariSoapNamespaces.Tns + "password", _options.Password),
                imageInfo));
    }

    private static XDocument WrapSoapBody(XElement bodyContent) =>
        new(new XElement(RahdariSoapNamespaces.SoapEnv + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", RahdariSoapNamespaces.SoapEnv),
            new XAttribute(XNamespace.Xmlns + "tns", RahdariSoapNamespaces.Tns),
            new XAttribute(XNamespace.Xmlns + "ttow", RahdariSoapNamespaces.Ttow),
            new XAttribute(XNamespace.Xmlns + "arr", RahdariSoapNamespaces.Arr),
            new XElement(RahdariSoapNamespaces.SoapEnv + "Body", bodyContent)));

    private async Task<string> PostSoapAsync(string soapAction, XDocument envelope, CancellationToken ct)
    {
        _logger.LogInformation(
            "Rahdari outbound {Action}: {Payload}",
            soapAction,
            RahdariOutboundLogger.SanitizeEnvelope(envelope));

        var client = _httpClientFactory.CreateClient("Rahdari");
        using var content = new StringContent(envelope.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
        content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.ServiceUrl) { Content = content };
        request.Headers.Add("SOAPAction", soapAction);

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            var explained = RahdariResponseInterpreter.ExplainSoapFault(body, soapAction);
            _logger.LogError(
                "Rahdari {Action} HTTP {Status}: {Summary}",
                soapAction,
                (int)response.StatusCode,
                explained.Summary);
            _syncStatus.RecordFailure(soapAction, explained);
            throw new HttpRequestException($"Rahdari HTTP {(int)response.StatusCode}: {explained.Summary}");
        }

        return body;
    }

    private static string? ParseLoginToken(string xml)
    {
        var doc = XDocument.Parse(xml);
        return doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Result")?.Value;
    }

    private static AddTtoResult ParseAddTtoResult(string xml)
    {
        var doc = XDocument.Parse(xml);
        var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "addTTOInfoResult");
        return result == null
            ? new AddTtoResult(-1, "Invalid response", 0)
            : ParseResponseWrapper(result);
    }

    private static Batch2ParseResult ParseBatch2Result(string xml)
    {
        var summary = SummariseResponse(xml);
        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (System.Xml.XmlException ex)
        {
            return new Batch2ParseResult(-1, Array.Empty<InquiryInfoResult>(), summary, Truncate(xml));
        }

        var wrapper = doc.Descendants().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, "addTTOInfoBatch2Result", StringComparison.OrdinalIgnoreCase));

        if (wrapper == null)
            return new Batch2ParseResult(-1, Array.Empty<InquiryInfoResult>(), summary, Truncate(xml));

        var errorCode = ReadDecimal(wrapper, "ErrorCode");

        // Never discard items just because the wrapper flags a failure: some deployments return
        // per-record InquiryInfo alongside a non-zero wrapper code (partial acceptance).
        var items = wrapper.Descendants()
            .Where(e => string.Equals(e.Name.LocalName, "InquiryInfo", StringComparison.OrdinalIgnoreCase))
            .Select(ParseInquiryInfo)
            .ToList();

        return new Batch2ParseResult(errorCode, items, summary, Truncate(xml));
    }

    /// <summary>
    /// One-line, base64-free description of a batch response, safe to write to logs.
    /// Without this the operator could only see "no InquiryInfo" and had no way to tell a
    /// rejected batch from an expired token or a genuinely empty result.
    /// </summary>
    private static string SummariseResponse(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var wrapper = doc.Descendants().FirstOrDefault(e =>
                string.Equals(e.Name.LocalName, "addTTOInfoBatch2Result", StringComparison.OrdinalIgnoreCase));

            if (wrapper == null)
                return "no addTTOInfoBatch2Result wrapper";

            var errorCode = ReadDecimal(wrapper, "ErrorCode");
            var success = wrapper.Elements()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, "IsSuccessfull", StringComparison.OrdinalIgnoreCase))?.Value;
            var itemCount = wrapper.Descendants()
                .Count(e => string.Equals(e.Name.LocalName, "InquiryInfo", StringComparison.OrdinalIgnoreCase));

            return $"ErrorCode={errorCode} ({ItsErrorCodes.Describe(errorCode)}) IsSuccessfull={success ?? "-"} items={itemCount}";
        }
        catch (Exception ex) when (ex is System.Xml.XmlException)
        {
            return $"unparseable response ({ex.Message})";
        }
    }

    private static string Truncate(string value) =>
        value.Length <= 2000 ? value : value[..2000] + "…";

    private static InquiryInfoResult ParseInquiryInfo(XElement element)
    {
        long Read(string name) => ReadDecimal(element, name);
        return new InquiryInfoResult(
            ReferenceNo: Read("REFERENCENO") != 0 ? Read("REFERENCENO") : Read("ReferenceNo"),
            ValidationCode: Read("VALIDATIONCODE") != 0 ? Read("VALIDATIONCODE") : Read("ValidationCode"),
            PassInfoId: Read("PassInfoId"),
            DeviceCode: Read("DeviceCode"),
            CompanyCode: Read("CompanyCode"),
            PackId: Read("PackId"),
            Reserved8: Read("Reserved8"),
            Reserved9: Read("Reserved9"),
            Reserved11: Read("Reserved11"),
            RectCenterX: ReadDecimalElement(element, "RectCenterX"),
            RectCenterY: ReadDecimalElement(element, "RectCenterY"),
            RectWidth: ReadDecimalElement(element, "RectWidth"),
            RectHeight: ReadDecimalElement(element, "RectHeight"),
            IsDoubleImage: Read("IsDoubleImage"));
    }

    private static AddImageResult ParseAddImageResult(string xml)
    {
        var doc = XDocument.Parse(xml);
        var wrapper = doc.Descendants().FirstOrDefault(e =>
            e.Name.LocalName is "AddImageResult" or "addImageResult");

        if (wrapper == null)
            return new AddImageResult(-1, "Invalid AddImage response", 0);

        var parsed = ParseResponseWrapper(wrapper);
        return new AddImageResult(parsed.ErrorCode, parsed.ErrorDesc, parsed.SysError);
    }

    private static AddTtoResult ParseResponseWrapper(XElement wrapper)
    {
        var errorCode = ReadDecimal(wrapper, "ErrorCode");
        var errorDesc = wrapper.Elements().FirstOrDefault(e => e.Name.LocalName == "ErrorDesc")?.Value ?? string.Empty;
        var sysError = ReadDecimal(wrapper, "SysError");
        var reference = ReadDecimal(wrapper, "Result");
        return new AddTtoResult(errorCode, errorDesc, sysError, reference);
    }

    private static long ReadDecimal(XElement parent, string localName)
    {
        var value = parent.Elements().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))?.Value;

        return long.TryParse(value, out var parsed) ? parsed : 0;
    }

    private static decimal ReadDecimalElement(XElement parent, string localName)
    {
        var value = parent.Elements().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))?.Value;

        return decimal.TryParse(value, out var parsed) ? parsed : 0;
    }

    public static bool IsTtoSuccess(long validationCode) => ItsErrorCodes.IsTtoSuccess(validationCode);

    public static bool IsImageSuccess(AddImageResult result) =>
        ItsErrorCodes.IsAddImageSuccess(result.ErrorCode, result.SysError);

    public static void EnsureTtoSuccess(long photoId, long validationCode, string? context = null)
    {
        if (IsTtoSuccess(validationCode))
            return;

        throw new RahdariSendException(
            photoId,
            $"Rahdari rejected PhotoId={photoId}: {ItsErrorCodes.Describe(validationCode)}{FormatContext(context)}");
    }

    public static void EnsureImageSuccess(long photoId, AddImageResult result)
    {
        if (IsImageSuccess(result))
            return;

        throw new RahdariSendException(
            photoId,
            $"Rahdari AddImage rejected PhotoId={photoId}: {ItsErrorCodes.Describe(result.ErrorCode)}",
            itsErrorCode: result.ErrorCode);
    }

    public static bool IsSuccess(AddTtoResult result) => IsTtoSuccess(result.ErrorCode);

    public static void EnsureSuccess(long photoId, AddTtoResult result)
    {
        if (IsSuccess(result))
            return;

        throw new RahdariSendException(
            photoId,
            $"Rahdari rejected PhotoId={photoId}: {ItsErrorCodes.Describe(result.ErrorCode)} ({result.ErrorDesc})");
    }

    private static string FormatContext(string? context) =>
        string.IsNullOrWhiteSpace(context) ? string.Empty : $" [{context}]";

    private sealed record Batch2ParseResult(
        long ErrorCode,
        IReadOnlyList<InquiryInfoResult> Items,
        string ResponseSummary,
        string RawResponse);
}

/// <summary>
/// Outcome of an addTTOInfoBatch2 call. <see cref="Items"/> may legitimately be empty while
/// <see cref="ErrorCode"/> is zero — that is an <b>unconfirmed</b> outcome, not a rejection,
/// and the caller must not dead-letter the records because of it.
/// </summary>
public sealed record BatchSendResult(
    IReadOnlyList<InquiryInfoResult> Items,
    long ErrorCode,
    string ResponseSummary);

public sealed record AddTtoResult(long ErrorCode, string ErrorDesc, long SysError, long Reference = 0);

public sealed record InquiryInfoResult(
    long ReferenceNo,
    long ValidationCode,
    long PassInfoId,
    long DeviceCode,
    long CompanyCode,
    long PackId,
    long Reserved8 = 0,
    long Reserved9 = 0,
    long Reserved11 = 0,
    decimal RectCenterX = 0,
    decimal RectCenterY = 0,
    decimal RectWidth = 0,
    decimal RectHeight = 0,
    long IsDoubleImage = 0);

public sealed record AddImageResult(long ErrorCode, string ErrorDesc, long SysError);
