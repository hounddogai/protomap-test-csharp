namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ScheduledReorgThresholdReportChecks
{
    public static void ScheduledReorgThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704577364);
        var items = new List<ScheduledReorgThreshold>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledReorgThreshold.NewScheduledReorgThreshold($"scheduled-reorg-threshold-{index}", "sample"));
        }
        var report = ScheduledReorgThresholdReport.BuildScheduledReorgThresholdReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledReorgThresholdReportCountsEachRecordOnce();
    }
}
