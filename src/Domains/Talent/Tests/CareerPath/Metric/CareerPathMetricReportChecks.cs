namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathMetricReportChecks
{
    public static void CareerPathMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703695856);
        var items = new List<CareerPathMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CareerPathMetric.NewCareerPathMetric($"career-path-metric-{index}", "sample"));
        }
        var report = CareerPathMetricReport.BuildCareerPathMetricReport("test", items, clock);
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
        CareerPathMetricReportCountsEachRecordOnce();
    }
}
