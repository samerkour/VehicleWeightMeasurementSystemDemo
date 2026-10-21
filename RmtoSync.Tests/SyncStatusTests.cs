using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Services;
using Xunit;

namespace RmtoSync.Tests;

/// <summary>
/// Counters and liveness fields backing the operator window (MainForm).
/// </summary>
public sealed class SyncStatusTests
{
    [Fact]
    public void RecordSent_counts_once_and_resets_consecutive_failures()
    {
        var status = new RahdariSyncStatus();
        status.RecordFailure("send", Failure());
        status.RecordFailure("send", Failure());

        Assert.Equal(2, status.FailedCount);
        Assert.Equal(2, status.ConsecutiveFailures);

        status.RecordSent();

        Assert.Equal(1, status.SentCount);
        Assert.Equal(0, status.ConsecutiveFailures);
        Assert.NotNull(status.LastSuccessAt);
    }

    [Fact]
    public void LastEvent_is_the_newest_entry_and_recent_list_is_capped()
    {
        var status = new RahdariSyncStatus();
        for (var i = 0; i < 60; i++)
            status.RecordSuccess("addTTOInfo", $"ok {i}", photoId: i, validationCode: 0);

        Assert.Equal(50, status.RecentEvents.Count);
        Assert.Equal(59, status.LastEvent!.Explanation.PhotoId);
        Assert.All(status.RecentEvents, e => Assert.True(e.IsSuccess));
    }

    [Fact]
    public void RecordCycleCompleted_tracks_liveness_without_raising_changed()
    {
        var status = new RahdariSyncStatus();
        var changes = 0;
        status.Changed += () => changes++;

        var before = DateTime.Now;
        status.RecordCycleCompleted(7, TimeSpan.FromSeconds(1.5));

        Assert.Equal(7, status.LastCycleProcessed);
        Assert.Equal(1.5, status.LastCycleDuration.TotalSeconds, 3);
        Assert.Equal(1, status.CycleCount);
        Assert.NotNull(status.LastCycleAt);
        Assert.True(status.LastCycleAt >= before);
        Assert.Equal(0, changes);
    }

    [Fact]
    public async Task QueueStatusReader_returns_null_without_connection_string()
    {
        Assert.Null(await QueueStatusReader.ReadAsync(
            null, 3, TimeSpan.FromSeconds(30), requireSensorData: false, CancellationToken.None));

        Assert.Null(await QueueStatusReader.ReadAsync(
            "   ", 3, TimeSpan.FromSeconds(30), requireSensorData: false, CancellationToken.None));
    }

    private static RahdariResultExplanation Failure() => new(
        Title: "خطای ارسال",
        Summary: "boom",
        Operation: "send/AddTtoWithImages",
        IsSuccess: false);
}
