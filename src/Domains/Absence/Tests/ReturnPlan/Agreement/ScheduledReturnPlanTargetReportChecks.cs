namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ScheduledReturnPlanTargetReportChecks
{
    public static void ScheduledReturnPlanTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707359950);
        var items = new List<ScheduledReturnPlanTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget($"scheduled-return-plan-target-{index}", "sample"));
        }
        var report = ScheduledReturnPlanTargetReport.BuildScheduledReturnPlanTargetReport("test", items, clock);
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
        ScheduledReturnPlanTargetReportCountsEachRecordOnce();
    }
}
