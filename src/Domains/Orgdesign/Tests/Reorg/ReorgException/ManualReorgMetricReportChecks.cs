namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ManualReorgMetricReportChecks
{
    public static void ManualReorgMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707514487);
        var items = new List<ManualReorgMetric>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualReorgMetric.NewManualReorgMetric($"manual-reorg-metric-{index}", "sample"));
        }
        var report = ManualReorgMetricReport.BuildManualReorgMetricReport("test", items, clock);
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
        ManualReorgMetricReportCountsEachRecordOnce();
    }
}
