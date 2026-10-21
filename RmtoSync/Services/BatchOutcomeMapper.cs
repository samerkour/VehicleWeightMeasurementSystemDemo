using RmtoSync.Models;

namespace RmtoSync.Services;

/// <summary>
/// نتیجه‌ی یک رکورد را از پاسخ <c>addTTOInfoBatch2</c> استخراج می‌کند.
/// <para>
/// قرارداد ITS: «Result یک آرایه از InquiryInfo است، به ازای هر رکورد» و هر عضو
/// <c>REFERENCENO</c> را برمی‌گرداند. اما در عمل بعضی نسخه‌های سرور
/// <c>REFERENCENO</c> را صفر/خالی برمی‌گردانند یا اصلاً InquiryInfo نمی‌فرستند.
/// در چنین حالتی نتیجه <b>نامعلوم</b> است، نه «رد شده». این کلاس هرگز نتیجه‌ی نامعلوم را
/// به «موفق» تبدیل نمی‌کند و آن را هم «ناموفق قطعی» حساب نمی‌کند؛ فقط آن را علامت می‌زند تا
/// لایه‌ی بالاتر با یک فراخوانی معتبر (addTTOInfo تکی) وضعیت را قطعی کند.
/// </para>
/// </summary>
public static class BatchOutcomeMapper
{
    public sealed record Outcome(
        long PhotoId,
        InquiryInfoResult? Inquiry,
        bool IsUnconfirmed,
        string Reason)
    {
        public bool IsMatched => Inquiry is not null;
    }

    /// <summary>
    /// یک نتیجه به ازای هر رکوردِ ارسال‌شده، به همان ترتیب ورودی.
    /// تطبیق اول بر اساس <c>ReferenceNo</c> است. اگر سرور برای هیچ رکوردی reference برنگرداند
    /// ولی تعداد پاسخ‌ها با تعداد درخواست‌ها برابر باشد، طبق قرارداد «one per record» تطبیق
    /// موقعیتی انجام می‌شود. در هر حالت دیگری نتیجه نامعلوم است.
    /// </summary>
    public static IReadOnlyList<Outcome> Map(
        IReadOnlyList<TtoPayload> sent,
        IReadOnlyList<InquiryInfoResult> received)
    {
        ArgumentNullException.ThrowIfNull(sent);
        ArgumentNullException.ThrowIfNull(received);

        var byReference = new Dictionary<long, InquiryInfoResult>(sent.Count);
        foreach (var item in received)
        {
            if (item.ReferenceNo > 0)
                byReference.TryAdd(item.ReferenceNo, item);
        }

        // تطبیق موقعیتی فقط وقتی امن است که سرور هیچ reference قابل‌استفاده‌ای برنگردانده باشد.
        // اگر reference برگشته ولی با هیچ رکوردی نمی‌خواند، حدس زدن یعنی احتمال ثبت موفقیت
        // برای رکورد اشتباه — بنابراین نامعلوم می‌ماند.
        var canMatchByPosition =
            received.Count == sent.Count &&
            received.Count > 0 &&
            received.All(r => r.ReferenceNo <= 0);

        var outcomes = new List<Outcome>(sent.Count);

        for (var index = 0; index < sent.Count; index++)
        {
            var photoId = sent[index].ReferenceNo;

            if (byReference.TryGetValue(photoId, out var inquiry))
            {
                outcomes.Add(new Outcome(photoId, inquiry, false, "matched by ReferenceNo"));
                continue;
            }

            if (canMatchByPosition)
            {
                outcomes.Add(new Outcome(
                    photoId,
                    received[index],
                    false,
                    "matched by position — server returned no REFERENCENO"));
                continue;
            }

            var reason = received.Count == 0
                ? "server returned no InquiryInfo"
                : $"server returned {received.Count} InquiryInfo for {sent.Count} sent record(s) with unusable REFERENCENO";

            outcomes.Add(new Outcome(photoId, null, true, reason));
        }

        return outcomes;
    }
}
