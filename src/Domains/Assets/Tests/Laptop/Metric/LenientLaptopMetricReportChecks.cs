namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientLaptopMetricReportChecks
{
    public static void LenientLaptopMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708561062);
        var items = new List<LenientLaptopMetric>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientLaptopMetric.NewLenientLaptopMetric($"lenient-laptop-metric-{index}", "sample"));
        }
        var report = LenientLaptopMetricReport.BuildLenientLaptopMetricReport("test", items, clock);
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
        LenientLaptopMetricReportCountsEachRecordOnce();
    }
}
