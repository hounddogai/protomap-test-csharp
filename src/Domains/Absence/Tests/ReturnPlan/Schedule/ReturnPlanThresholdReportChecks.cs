namespace Acme.Workforce.Absence.Tests.ReturnPlan.Schedule;

using Acme.Workforce.Absence.ReturnPlan.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReturnPlanThresholdReportChecks
{
    public static void ReturnPlanThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700311788);
        var items = new List<ReturnPlanThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ReturnPlanThreshold.NewReturnPlanThreshold($"return-plan-threshold-{index}", "sample"));
        }
        var report = ReturnPlanThresholdReport.BuildReturnPlanThresholdReport("test", items, clock);
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
        ReturnPlanThresholdReportCountsEachRecordOnce();
    }
}
