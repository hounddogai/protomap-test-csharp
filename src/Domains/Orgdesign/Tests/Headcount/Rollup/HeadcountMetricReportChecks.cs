namespace Acme.Workforce.Orgdesign.Tests.Headcount.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class HeadcountMetricReportChecks
{
    public static void HeadcountMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701624999);
        var items = new List<HeadcountMetric>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HeadcountMetric.NewHeadcountMetric($"headcount-metric-{index}", "sample"));
        }
        var report = HeadcountMetricReport.BuildHeadcountMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        HeadcountMetricReportCountsEachRecordOnce();
    }
}
