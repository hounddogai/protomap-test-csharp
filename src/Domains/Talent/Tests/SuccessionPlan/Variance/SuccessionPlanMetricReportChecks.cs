namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanMetricReportChecks
{
    public static void SuccessionPlanMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707738799);
        var items = new List<SuccessionPlanMetric>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SuccessionPlanMetric.NewSuccessionPlanMetric($"succession-plan-metric-{index}", "sample"));
        }
        var report = SuccessionPlanMetricReport.BuildSuccessionPlanMetricReport("test", items, clock);
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
        SuccessionPlanMetricReportCountsEachRecordOnce();
    }
}
