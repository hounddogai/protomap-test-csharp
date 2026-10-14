namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class StrictEquityGrantMetricReportChecks
{
    public static void StrictEquityGrantMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702745500);
        var items = new List<StrictEquityGrantMetric>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictEquityGrantMetric.NewStrictEquityGrantMetric($"strict-equity-grant-metric-{index}", "sample"));
        }
        var report = StrictEquityGrantMetricReport.BuildStrictEquityGrantMetricReport("test", items, clock);
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
        StrictEquityGrantMetricReportCountsEachRecordOnce();
    }
}
