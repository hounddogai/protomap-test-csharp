namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanScheduleReportChecks
{
    public static void ActionPlanScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708975983);
        var items = new List<ActionPlanSchedule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ActionPlanSchedule.NewActionPlanSchedule($"action-plan-schedule-{index}", "sample"));
        }
        var report = ActionPlanScheduleReport.BuildActionPlanScheduleReport("test", items, clock);
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
        ActionPlanScheduleReportCountsEachRecordOnce();
    }
}
