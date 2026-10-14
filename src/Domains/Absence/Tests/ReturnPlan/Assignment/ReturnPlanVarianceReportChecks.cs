namespace Acme.Workforce.Absence.Tests.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ReturnPlanVarianceReportChecks
{
    public static void ReturnPlanVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703674153);
        var items = new List<ReturnPlanVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ReturnPlanVariance.NewReturnPlanVariance($"return-plan-variance-{index}", "sample"));
        }
        var report = ReturnPlanVarianceReport.BuildReturnPlanVarianceReport("test", items, clock);
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
        ReturnPlanVarianceReportCountsEachRecordOnce();
    }
}
