namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeCheckpointReportChecks
{
    public static void AccessBadgeCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704661737);
        var items = new List<AccessBadgeCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AccessBadgeCheckpoint.NewAccessBadgeCheckpoint($"access-badge-checkpoint-{index}", "sample"));
        }
        var report = AccessBadgeCheckpointReport.BuildAccessBadgeCheckpointReport("test", items, clock);
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
        AccessBadgeCheckpointReportCountsEachRecordOnce();
    }
}
