using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RmtoSync.Its;

/// <summary>Maps Rahdari SOAP/HTTP responses to ITS guide (PDF) descriptions.</summary>
public static class RahdariResponseInterpreter
{
    public static RahdariResultExplanation ExplainValidationCode(long code) =>
        new(
            Title: code is >= 0 and <= 9 or 99 ? "نتیجه موفق یا هشدار" : "خطای اعتبارسنجی راهداری",
            Summary: ItsErrorCodes.Describe(code),
            ErrorCode: code,
            Source: "ITS فصل 3 — addTTOInfo / InquiryInfo");

    public static RahdariResultExplanation ExplainException(Exception ex, string? operation = null)
    {
        if (ex is HttpRequestException http && TryExtractSoapBody(http.Message, out var body))
            return ExplainSoapFault(body, operation);

        if (ex.InnerException is HttpRequestException inner && TryExtractSoapBody(inner.Message, out var innerBody))
            return ExplainSoapFault(innerBody, operation);

        var validation = TryExtractValidationCode(ex.Message);
        if (validation.HasValue)
        {
            var explained = ExplainValidationCode(validation.Value);
            return explained with
            {
                TechnicalDetail = ex.Message,
                Operation = operation ?? explained.Operation
            };
        }

        return new RahdariResultExplanation(
            Title: "خطای ارسال",
            Summary: ex.Message,
            TechnicalDetail: ex.ToString(),
            Operation: operation);
    }

    public static RahdariResultExplanation ExplainSoapFault(string soapBody, string? operation = null)
    {
        try
        {
            var doc = XDocument.Parse(soapBody);
            var fault = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
            if (fault != null)
            {
                var faultCode = fault.Elements().FirstOrDefault(e => e.Name.LocalName == "faultcode")?.Value ?? string.Empty;
                var faultString = fault.Elements().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value ?? string.Empty;
                return ExplainSchemaOrFault(faultCode, faultString, soapBody, operation);
            }

            var errorCode = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName.Equals("ErrorCode", StringComparison.OrdinalIgnoreCase))?.Value;

            if (long.TryParse(errorCode, out var parsed))
            {
                var explained = ExplainValidationCode(parsed);
                var errorDesc = doc.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName.Equals("ErrorDesc", StringComparison.OrdinalIgnoreCase))?.Value;
                return explained with
                {
                    Summary = string.IsNullOrWhiteSpace(errorDesc) ? explained.Summary : $"{explained.Summary} — {errorDesc}",
                    TechnicalDetail = soapBody,
                    Operation = operation
                };
            }
        }
        catch
        {
            // fall through
        }

        return new RahdariResultExplanation(
            Title: "پاسخ نامعتبر راهداری",
            Summary: "سرور پاسخ SOAP قابل parse نداد.",
            TechnicalDetail: soapBody,
            Operation: operation);
    }

    private static RahdariResultExplanation ExplainSchemaOrFault(
        string faultCode,
        string faultString,
        string soapBody,
        string? operation)
    {
        if (faultString.Contains("SchemaValidationError", StringComparison.OrdinalIgnoreCase)
            || faultCode.Contains("SchemaValidationError", StringComparison.OrdinalIgnoreCase))
        {
            var element = Regex.Match(faultString, @"Element '\{[^}]+\}([^']+)'").Groups[1].Value;
            var expected = Regex.Match(faultString, @"Expected is \( \{[^}]+\}([^)]+) \)").Groups[1].Value;

            var summary = element switch
            {
                "TTOInfo" => "ساختار XML batch اشتباه است: باید ttow:ttoInfo باشد نه tempuri:TTOInfo (ITS فصل 5 / WSDL).",
                "score" => "فیلد score در ارسال batch مجاز نیست یا جای آن در XML اشتباه است (نمونه‌های فصل 5 batch بدون score).",
                "cARCLASS13" => "فیلد cARCLASS13 در این نوع ارسال یا ترتیب XML مجاز نیست.",
                _ when !string.IsNullOrEmpty(element)
                    => $"فیلد XML «{element}» با schema سرور سازگار نیست.",
                _ => "ساختار XML درخواست با schema وب‌سرویس راهداری مطابقت ندارد."
            };

            if (!string.IsNullOrEmpty(expected))
                summary += $" انتظار: {expected.Trim()}.";

            return new RahdariResultExplanation(
                Title: "خطای Schema (XML)",
                Summary: summary,
                TechnicalDetail: faultString,
                Operation: operation,
                Source: "ITS فصل 5 — نمونه‌های SOAP");
        }

        return new RahdariResultExplanation(
            Title: "خطای SOAP",
            Summary: string.IsNullOrWhiteSpace(faultString) ? faultCode : faultString,
            TechnicalDetail: soapBody,
            Operation: operation,
            Source: "پاسخ Fault سرور");
    }

    private static bool TryExtractSoapBody(string message, out string body)
    {
        var idx = message.IndexOf("<?xml", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            body = message[idx..];
            return true;
        }

        body = string.Empty;
        return false;
    }

    private static long? TryExtractValidationCode(string message)
    {
        var match = Regex.Match(message, @"ValidationCode[=:]?\s*(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && long.TryParse(match.Groups[1].Value, out var code))
            return code;

        match = Regex.Match(message, @"ErrorCode[=:]?\s*(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && long.TryParse(match.Groups[1].Value, out code))
            return code;

        return null;
    }

    public static string FormatForDialog(RahdariResultExplanation explanation)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(explanation.Operation))
            lines.Add($"عملیات: {explanation.Operation}");
        if (explanation.PhotoId.HasValue)
            lines.Add($"PhotoId: {explanation.PhotoId}");
        if (explanation.ErrorCode.HasValue)
            lines.Add($"کد: {explanation.ErrorCode}");
        if (!string.IsNullOrWhiteSpace(explanation.Source))
            lines.Add($"مرجع: {explanation.Source}");

        var kind = RahdariOutcomeClassifier.Classify(explanation, explanation.IsSuccess, hasEvent: true, serviceRunning: false);
        var hint = RahdariOutcomeClassifier.Present(kind).DocHint;
        if (!string.IsNullOrWhiteSpace(hint))
        {
            lines.Add(string.Empty);
            lines.Add($"— راهنما —");
            lines.Add(hint);
        }

        lines.Add(string.Empty);
        lines.Add(explanation.Summary);
        if (!string.IsNullOrWhiteSpace(explanation.TechnicalDetail))
        {
            lines.Add(string.Empty);
            lines.Add("— جزئیات فنی —");
            lines.Add(explanation.TechnicalDetail);
        }

        return string.Join(Environment.NewLine, lines);
    }
}

public sealed record RahdariResultExplanation(
    string Title,
    string Summary,
    string? TechnicalDetail = null,
    long? ErrorCode = null,
    long? PhotoId = null,
    string? Operation = null,
    string? Source = null,
    bool IsSuccess = false);
