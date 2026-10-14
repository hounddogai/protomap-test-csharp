namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictReturnPlanMetricReportChecks
{
    public static void StrictReturnPlanMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708104542);
        var items = new List<StrictReturnPlanMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictReturnPlanMetric.NewStrictReturnPlanMetric($"strict-return-plan-metric-{index}", "sample"));
        }
        var report = StrictReturnPlanMetricReport.BuildStrictReturnPlanMetricReport("test", items, clock);
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
        StrictReturnPlanMetricReportCountsEachRecordOnce();
    }
}
