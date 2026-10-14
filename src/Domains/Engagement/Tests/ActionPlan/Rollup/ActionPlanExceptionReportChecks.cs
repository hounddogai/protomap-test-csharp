namespace Acme.Workforce.Engagement.Tests.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ActionPlanExceptionReportChecks
{
    public static void ActionPlanExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704199595);
        var items = new List<ActionPlanException>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ActionPlanException.NewActionPlanException($"action-plan-exception-{index}", "sample"));
        }
        var report = ActionPlanExceptionReport.BuildActionPlanExceptionReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ActionPlanExceptionReportCountsEachRecordOnce();
    }
}
