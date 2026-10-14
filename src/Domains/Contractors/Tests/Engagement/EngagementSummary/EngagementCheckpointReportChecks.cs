namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class EngagementCheckpointReportChecks
{
    public static void EngagementCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705999882);
        var items = new List<EngagementCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EngagementCheckpoint.NewEngagementCheckpoint($"engagement-checkpoint-{index}", "sample"));
        }
        var report = EngagementCheckpointReport.BuildEngagementCheckpointReport("test", items, clock);
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
        EngagementCheckpointReportCountsEachRecordOnce();
    }
}
