namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMetricReportChecks
{
    public static void ReorgMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700407168);
        var items = new List<ReorgMetric>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ReorgMetric.NewReorgMetric($"reorg-metric-{index}", "sample"));
        }
        var report = ReorgMetricReport.BuildReorgMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ReorgMetricReportCountsEachRecordOnce();
    }
}
