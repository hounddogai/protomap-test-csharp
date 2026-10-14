namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanEstimateReportChecks
{
    public static void ReturnPlanEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702093057);
        var items = new List<ReturnPlanEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ReturnPlanEstimate.NewReturnPlanEstimate($"return-plan-estimate-{index}", "sample"));
        }
        var report = ReturnPlanEstimateReport.BuildReturnPlanEstimateReport("test", items, clock);
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
        ReturnPlanEstimateReportCountsEachRecordOnce();
    }
}
