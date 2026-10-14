namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanTargetReportChecks
{
    public static void ReturnPlanTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701224838);
        var items = new List<ReturnPlanTarget>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ReturnPlanTarget.NewReturnPlanTarget($"return-plan-target-{index}", "sample"));
        }
        var report = ReturnPlanTargetReport.BuildReturnPlanTargetReport("test", items, clock);
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
        ReturnPlanTargetReportCountsEachRecordOnce();
    }
}
