namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanMetricReportChecks
{
    public static void DefaultActionPlanMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702301166);
        var items = new List<DefaultActionPlanMetric>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultActionPlanMetric.NewDefaultActionPlanMetric($"default-action-plan-metric-{index}", "sample"));
        }
        var report = DefaultActionPlanMetricReport.BuildDefaultActionPlanMetricReport("test", items, clock);
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
        DefaultActionPlanMetricReportCountsEachRecordOnce();
    }
}
