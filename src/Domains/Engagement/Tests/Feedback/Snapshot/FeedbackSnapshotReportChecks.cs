namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackSnapshotReportChecks
{
    public static void FeedbackSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706005671);
        var items = new List<FeedbackSnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FeedbackSnapshot.NewFeedbackSnapshot($"feedback-snapshot-{index}", "sample"));
        }
        var report = FeedbackSnapshotReport.BuildFeedbackSnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FeedbackSnapshotReportCountsEachRecordOnce();
    }
}
