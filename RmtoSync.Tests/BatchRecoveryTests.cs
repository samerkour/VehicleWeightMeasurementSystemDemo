using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Models;
using RmtoSync.Services;
using Xunit;

namespace RmtoSync.Tests;

/// <summary>
/// رگرسیون «Batch response missing InquiryInfo»:
/// نبودِ InquiryInfo در پاسخ addTTOInfoBatch2 یک نتیجه‌ی <b>نامعلوم</b> است، نه رد شدن رکورد.
/// کد قدیمی این حالت را شکست قطعی می‌شمرد، سه تلاش را می‌سوزاند و رکورد را برای همیشه رها
/// می‌کرد. این تست‌ها تضمین می‌کنند که:
/// ۱) تطبیق بر اساس ReferenceNo و در نبودِ آن بر اساس ترتیب انجام شود؛
/// ۲) نتیجه‌ی نامعلوم هرگز «موفق» تلقی نشود؛
/// ۳) خطای واقعی سرور (کد خطا) دیگر به پیام گمراه‌کننده تبدیل نشود؛
/// ۴) توکن منقضی باعث Login مجدد شود.
/// </summary>
public sealed class BatchRecoveryTests
{
    // ── BatchOutcomeMapper ────────────────────────────────────────────────────────────

    [Fact]
    public void Map_matches_every_record_by_reference_number()
    {
        var sent = new[] { Payload(101), Payload(102) };
        var received = new[] { Inquiry(102, 0), Inquiry(101, 99) };

        var outcomes = BatchOutcomeMapper.Map(sent, received);

        Assert.Equal(2, outcomes.Count);
        Assert.All(outcomes, o => Assert.False(o.IsUnconfirmed));
        Assert.Equal(99, outcomes.Single(o => o.PhotoId == 101).Inquiry!.ValidationCode);
        Assert.Equal(0, outcomes.Single(o => o.PhotoId == 102).Inquiry!.ValidationCode);
    }

    [Fact]
    public void Map_falls_back_to_position_only_when_server_returns_no_reference()
    {
        var sent = new[] { Payload(201), Payload(202) };
        var received = new[] { Inquiry(0, 99), Inquiry(0, 101) };

        var outcomes = BatchOutcomeMapper.Map(sent, received);

        Assert.All(outcomes, o => Assert.False(o.IsUnconfirmed));
        Assert.Equal(99, outcomes[0].Inquiry!.ValidationCode);
        Assert.Equal(101, outcomes[1].Inquiry!.ValidationCode);
        Assert.Contains("position", outcomes[0].Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Map_never_guesses_when_reference_numbers_are_present_but_do_not_match()
    {
        var sent = new[] { Payload(301), Payload(302) };
        var received = new[] { Inquiry(9001, 0), Inquiry(9002, 0) };

        var outcomes = BatchOutcomeMapper.Map(sent, received);

        Assert.All(outcomes, o => Assert.True(o.IsUnconfirmed));
        Assert.All(outcomes, o => Assert.Null(o.Inquiry));
    }

    [Fact]
    public void Map_reports_unconfirmed_when_response_is_empty()
    {
        var outcomes = BatchOutcomeMapper.Map(new[] { Payload(401) }, Array.Empty<InquiryInfoResult>());

        var outcome = Assert.Single(outcomes);
        Assert.True(outcome.IsUnconfirmed);
        Assert.Contains("no InquiryInfo", outcome.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Map_marks_partial_response_as_unconfirmed_without_misattributing()
    {
        var sent = new[] { Payload(501), Payload(502), Payload(503) };
        var received = new[] { Inquiry(502, 0) };

        var outcomes = BatchOutcomeMapper.Map(sent, received);

        Assert.Equal(3, outcomes.Count);
        Assert.NotNull(outcomes.Single(o => o.PhotoId == 502).Inquiry);
        Assert.All(outcomes.Where(o => o.PhotoId != 502), o => Assert.True(o.IsUnconfirmed));
    }

    // ── RahdariTtoClient.SendBatchAsync ───────────────────────────────────────────────

    [Fact]
    public async Task SendBatch_parses_inquiry_items_with_reference_and_validation_code()
    {
        var handler = new ScriptedHandler(action => action switch
        {
            "Login" => LoginOk("TOKEN"),
            "addTTOInfoBatch2" => BatchResponse(
                errorCode: 0,
                items: """
                       <InquiryInfo xmlns="http://schemas.datacontract.org/2004/07/TTOWS.Library.Models">
                         <REFERENCENO>7001</REFERENCENO>
                         <VALIDATIONCODE>99</VALIDATIONCODE>
                         <PassInfoId>551</PassInfoId>
                       </InquiryInfo>
                       """),
            _ => throw new InvalidOperationException(action)
        });

        var client = CreateClient(handler);
        var result = await client.SendBatchAsync(new[] { Payload(7001) }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(7001, item.ReferenceNo);
        Assert.Equal(99, item.ValidationCode);
        Assert.Equal(551, item.PassInfoId);
        Assert.Equal(0, result.ErrorCode);
        Assert.Contains("items=1", result.ResponseSummary);
    }

    [Fact]
    public async Task SendBatch_reports_empty_body_as_unconfirmed_not_as_exception()
    {
        var handler = new ScriptedHandler(action => action switch
        {
            "Login" => LoginOk("TOKEN"),
            "addTTOInfoBatch2" => BatchResponse(errorCode: 0, items: string.Empty),
            _ => throw new InvalidOperationException(action)
        });

        var client = CreateClient(handler);
        var result = await client.SendBatchAsync(new[] { Payload(7002) }, CancellationToken.None);

        // No items, but no exception: the caller must be able to distinguish
        // "server said nothing" from "server rejected".
        Assert.Empty(result.Items);
        Assert.Equal(0, result.ErrorCode);
        Assert.Contains("items=0", result.ResponseSummary);
    }

    [Fact]
    public async Task SendBatch_surfaces_the_real_server_error_code_instead_of_missing_inquiry()
    {
        var handler = new ScriptedHandler(action => action switch
        {
            "Login" => LoginOk("TOKEN"),
            "addTTOInfoBatch2" => BatchResponse(errorCode: 108, items: string.Empty),
            _ => throw new InvalidOperationException(action)
        });

        var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<RahdariSendException>(
            () => client.SendBatchAsync(new[] { Payload(7003) }, CancellationToken.None));

        Assert.Equal(108L, ex.ItsErrorCode);
        Assert.Contains("108", ex.Message);
        Assert.DoesNotContain("missing InquiryInfo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1001)]
    [InlineData(1002)]
    public async Task SendBatch_re_logins_when_server_reports_expired_token(int expiredCode)
    {
        var batchCalls = 0;
        var loginCalls = 0;

        var handler = new ScriptedHandler(action =>
        {
            if (action == "Login")
            {
                loginCalls++;
                return LoginOk($"TOKEN{loginCalls}");
            }

            if (action == "addTTOInfoBatch2")
            {
                batchCalls++;
                return batchCalls == 1
                    ? BatchResponse(errorCode: expiredCode, items: string.Empty)
                    : BatchResponse(
                        errorCode: 0,
                        items: """
                               <InquiryInfo>
                                 <REFERENCENO>7004</REFERENCENO>
                                 <VALIDATIONCODE>0</VALIDATIONCODE>
                               </InquiryInfo>
                               """);
            }

            throw new InvalidOperationException(action);
        });

        var client = CreateClient(handler);
        var result = await client.SendBatchAsync(new[] { Payload(7004) }, CancellationToken.None);

        Assert.Equal(2, loginCalls);
        Assert.Equal(2, batchCalls);
        Assert.Equal(7004, Assert.Single(result.Items).ReferenceNo);
    }

    [Fact]
    public async Task Map_then_confirm_flow_registers_only_after_authoritative_single_send()
    {
        // End-to-end of the recovery rule at the parsing level: the batch says nothing,
        // so the outcome must be unconfirmed; the single addTTOInfo answer is authoritative.
        var handler = new ScriptedHandler(action => action switch
        {
            "Login" => LoginOk("TOKEN"),
            "addTTOInfoBatch2" => BatchResponse(errorCode: 0, items: string.Empty),
            "addTTOInfo" => """
                           <addTTOInfoResponse xmlns="http://tempuri.org/">
                             <addTTOInfoResult>
                               <ErrorCode>101</ErrorCode>
                               <ErrorDesc>duplicate</ErrorDesc>
                               <Result>8899</Result>
                             </addTTOInfoResult>
                           </addTTOInfoResponse>
                           """,
            _ => throw new InvalidOperationException(action)
        });

        var client = CreateClient(handler);
        var payload = Payload(7005);

        var batch = await client.SendBatchAsync(new[] { payload }, CancellationToken.None);
        var mapped = Assert.Single(BatchOutcomeMapper.Map(new[] { payload }, batch.Items));
        Assert.True(mapped.IsUnconfirmed);

        var confirm = await client.SendTtoOnlyAsync(payload, CancellationToken.None);
        Assert.Equal(Its.ItsErrorCodes.AddTto.Duplicate, confirm.ErrorCode);        Assert.Equal(8899, confirm.Reference);
    }

    // ── Routing: old records cannot use AddImage any more ─────────────────────────────

    [Fact]
    public void Router_sends_inline_images_when_the_addimage_window_has_already_passed()
    {
        var options = new RahdariOptions { AnprOnlyStation = true, ImageAttachWindowHours = 24 };
        var oldPhoto = Photo(passedHoursAgo: 72);

        var plan = TtoSendRouter.Plan(oldPhoto, options);

        Assert.Equal(TtoSendRouter.SendMode.SingleWithImages, plan.Mode);
    }

    [Fact]
    public void Router_prefers_batch_while_the_addimage_window_is_still_open()
    {
        var options = new RahdariOptions { AnprOnlyStation = true, ImageAttachWindowHours = 24 };
        var freshPhoto = Photo(passedHoursAgo: 1);

        var plan = TtoSendRouter.Plan(freshPhoto, options);

        Assert.Equal(TtoSendRouter.SendMode.BatchThenImage, plan.Mode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────────

    private static TtoPayload Payload(long reference) => new() { ReferenceNo = reference };

    private static InquiryInfoResult Inquiry(long reference, long validationCode) =>
        new(ReferenceNo: reference, ValidationCode: validationCode, PassInfoId: 0, DeviceCode: 0, CompanyCode: 0, PackId: 0);

    private static CameraPhotoRecord Photo(int passedHoursAgo) => new()
    {
        PhotoId = 9001,
        LineCode = "L1",
        LineId = 1,
        ImportedAt = DateTime.Now.AddHours(-passedHoursAgo),
        PlateP1 = "12",
        PlateP2 = "ا",
        PlateP3 = "345",
        PlateP4 = "67",
        FullPath = @"C:\images\test.jpg",
        CapturedAt = DateTime.Now.AddHours(-passedHoursAgo),
    };

    private static RahdariTtoClient CreateClient(HttpMessageHandler handler) =>
        new(
            Options.Create(new RahdariOptions { ServiceUrl = "http://localhost/rahnari" }),
            new SingleClientFactory(handler),
            new RahdariSyncStatus(),
            NullLogger<RahdariTtoClient>.Instance);

    private static string LoginOk(string token) =>
        $"""
         <LoginResponse xmlns="http://tempuri.org/">
           <LoginResult><Result>{token}</Result></LoginResult>
         </LoginResponse>
         """;

    private static string BatchResponse(long errorCode, string items) =>
        $"""
         <addTTOInfoBatch2Response xmlns="http://tempuri.org/">
           <addTTOInfoBatch2Result>
             <ErrorCode>{errorCode}</ErrorCode>
             <IsSuccessfull>{(errorCode == 0 ? "true" : "false")}</IsSuccessfull>
             <Result>{items}</Result>
           </addTTOInfoBatch2Result>
         </addTTOInfoBatch2Response>
         """;

    private sealed class SingleClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public SingleClientFactory(HttpMessageHandler handler) => _client = new HttpClient(handler);

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Func<string, string> _respond;

        public ScriptedHandler(Func<string, string> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var action = request.Headers.TryGetValues("SOAPAction", out var values)
                ? values.First()
                : string.Empty;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_respond(action), Encoding.UTF8, "text/xml"),
            };

            return Task.FromResult(response);
        }
    }
}
