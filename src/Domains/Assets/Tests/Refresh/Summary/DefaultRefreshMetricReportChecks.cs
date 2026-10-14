namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultRefreshMetricReportChecks
{
    public static void DefaultRefreshMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702805362);
        var items = new List<DefaultRefreshMetric>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultRefreshMetric.NewDefaultRefreshMetric($"default-refresh-metric-{index}", "sample"));
        }
        var report = DefaultRefreshMetricReport.BuildDefaultRefreshMetricReport("test", items, clock);
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
        DefaultRefreshMetricReportCountsEachRecordOnce();
    }
}
