namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PendingEngagementCheckpointReportChecks
{
    public static void PendingEngagementCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703189292);
        var items = new List<PendingEngagementCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingEngagementCheckpoint.NewPendingEngagementCheckpoint($"pending-engagement-checkpoint-{index}", "sample"));
        }
        var report = PendingEngagementCheckpointReport.BuildPendingEngagementCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingEngagementCheckpointReportCountsEachRecordOnce();
    }
}
