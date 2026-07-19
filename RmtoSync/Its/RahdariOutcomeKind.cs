namespace RmtoSync.Its;

/// <summary>
/// UI outcome buckets aligned with ITS guide (PDF) chapters 3-2 … 3-6 and chapter 1 process notes.
/// </summary>
public enum RahdariOutcomeKind
{
    /// <summary>No Rahdari response yet; service not started.</summary>
    Unknown,

    /// <summary>Service is running and polling; waiting for first terminal response.</summary>
    Waiting,

    /// <summary>ValidationCode 0, 1, 2 or duplicate 101 — accepted by Rahdari.</summary>
    Accepted,

    /// <summary>ValidationCode 99 — accepted without image; 24h window per chapter 3-3 / 1-4.</summary>
    AcceptedNeedsImage,

    /// <summary>Business validation rejection (102+), batch 1007/1008, image rules, etc.</summary>
    Rejected,

    /// <summary>Login 1000, token 1002, method 1011, user 128 — access/auth.</summary>
    AccessDenied,

    /// <summary>SOAP Fault / SchemaValidationError / malformed HTTP response.</summary>
    ProtocolError
}

public static class RahdariOutcomeClassifier
{
    private static readonly HashSet<long> AccessCodes =
    [
        ItsErrorCodes.Login.InvalidCredentials,
        ItsErrorCodes.AddTto.TokenExpired,
        ItsErrorCodes.AddTto.MethodNotAllowed,
        ItsErrorCodes.AddTto.UserNotAuthorized,
        ItsErrorCodes.AddImage.InvalidCredentials,
        ItsErrorCodes.AddImage.UserBlocked,
        ItsErrorCodes.AddImage.CredentialsExpired,
        ItsErrorCodes.AddImage.InvalidIp,
        ItsErrorCodes.Batch.MethodNotAllowed
    ];

    public static RahdariOutcomeKind Classify(
        RahdariResultExplanation? explanation,
        bool isSuccess,
        bool hasEvent,
        bool serviceRunning)
    {
        if (!hasEvent)
            return serviceRunning ? RahdariOutcomeKind.Waiting : RahdariOutcomeKind.Unknown;

        if (explanation is null)
            return RahdariOutcomeKind.Unknown;

        if (IsProtocolIssue(explanation))
            return RahdariOutcomeKind.ProtocolError;

        var code = explanation.ErrorCode;
        if (code.HasValue && AccessCodes.Contains(code.Value))
            return RahdariOutcomeKind.AccessDenied;

        if (isSuccess)
        {
            if (code == ItsErrorCodes.AddTto.OkAwaitingImage)
                return RahdariOutcomeKind.AcceptedNeedsImage;

            if (!code.HasValue || ItsErrorCodes.IsTtoSuccess(code.Value))
                return RahdariOutcomeKind.Accepted;
        }

        return RahdariOutcomeKind.Rejected;
    }

    public static RahdariOutcomePresentation Present(RahdariOutcomeKind kind) => kind switch
    {
        RahdariOutcomeKind.Waiting => new(
            kind,
            "در انتظار",
            "در انتظار پاسخ راهداری",
            Color.FromArgb(100, 181, 246),
            Color.FromArgb(13, 60, 97),
            "سرویس فعال است و منتظر اولین پاسخ از ترمینال راهداری.",
            "ITS فصل 1 — چرخه ارسال"),

        RahdariOutcomeKind.Accepted => new(
            kind,
            "پذیرفته شد",
            "پذیرفته شد",
            Color.FromArgb(129, 199, 132),
            Color.FromArgb(20, 60, 25),
            "تردد توسط راهداری پذیرفته شد (کدهای 0، 1، 2 یا تکراری 101).",
            "ITS فصل 3-3 — جدول خطای addTTOInfo"),

        RahdariOutcomeKind.AcceptedNeedsImage => new(
            kind,
            "نیاز به عکس",
            "پذیرش موقت — ارسال عکس",
            Color.FromArgb(255, 183, 77),
            Color.FromArgb(90, 45, 0),
            "تردد ثبت شد ولی بدون عکس ارسال شده (کد 99). تا 24 ساعت فرصت ارسال مجدد با عکس دارید.",
            "ITS فصل 3-3 کد 99 / فصل 1-4"),

        RahdariOutcomeKind.Rejected => new(
            kind,
            "رد شد",
            "رد توسط راهداری",
            Color.FromArgb(229, 115, 115),
            Color.White,
            "اعتبارسنجی راهداری رد کرد (کدهای 102 به بالا، batch 1007/1008، یا قوانین تصویر).",
            "ITS فصل 3-3 … 3-6 — جداول خطا"),

        RahdariOutcomeKind.AccessDenied => new(
            kind,
            "بدون دسترسی",
            "خطای دسترسی / احراز هویت",
            Color.FromArgb(186, 104, 200),
            Color.White,
            "نام کاربری، توکن، IP یا مجوز متد (1000، 1002، 1011، 128).",
            "ITS فصل 3-2 Login / فصل 3-3 کد 128"),

        RahdariOutcomeKind.ProtocolError => new(
            kind,
            "خطای XML",
            "خطای XML / ارتباط",
            Color.FromArgb(198, 40, 40),
            Color.White,
            "درخواست SOAP از نظر schema یا پروتکل پذیرفته نشد (قبل از اعتبارسنجی business).",
            "ITS فصل 5 — نمونه‌های SOAP / WSDL"),

        _ => new(
            RahdariOutcomeKind.Unknown,
            "نامشخص",
            "نامشخص",
            Color.FromArgb(255, 214, 102),
            Color.FromArgb(80, 60, 0),
            "هنوز پاسخی از راهداری دریافت نشده است.",
            "ITS — قبل از اولین فراخوانی")
    };

    public static string DialogTitle(RahdariOutcomeKind kind) =>
        Present(kind).LabelLong;

    private static bool IsProtocolIssue(RahdariResultExplanation explanation) =>
        explanation.Title.Contains("Schema", StringComparison.OrdinalIgnoreCase)
        || explanation.Title.Contains("SOAP", StringComparison.OrdinalIgnoreCase)
        || (explanation.Source?.Contains("فصل 5", StringComparison.Ordinal) == true
            && !explanation.IsSuccess);
}

public sealed record RahdariOutcomePresentation(
    RahdariOutcomeKind Kind,
    string ButtonText,
    string LabelLong,
    Color BackColor,
    Color ForeColor,
    string DocHint,
    string DocReference);
