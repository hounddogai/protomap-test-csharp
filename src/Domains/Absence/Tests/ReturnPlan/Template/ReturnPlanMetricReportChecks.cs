namespace Acme.Workforce.Absence.Tests.ReturnPlan.Template;

using Acme.Workforce.Absence.ReturnPlan.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ReturnPlanMetricReportChecks
{
    public static void ReturnPlanMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700177984);
        var items = new List<ReturnPlanMetric>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ReturnPlanMetric.NewReturnPlanMetric($"return-plan-metric-{index}", "sample"));
        }
        var report = ReturnPlanMetricReport.BuildReturnPlanMetricReport("test", items, clock);
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
        ReturnPlanMetricReportCountsEachRecordOnce();
    }
}
