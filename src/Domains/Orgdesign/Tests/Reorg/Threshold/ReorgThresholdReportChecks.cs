namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgThresholdReportChecks
{
    public static void ReorgThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702964660);
        var items = new List<ReorgThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ReorgThreshold.NewReorgThreshold($"reorg-threshold-{index}", "sample"));
        }
        var report = ReorgThresholdReport.BuildReorgThresholdReport("test", items, clock);
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
        ReorgThresholdReportCountsEachRecordOnce();
    }
}
