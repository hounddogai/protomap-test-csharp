namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanAssignmentReportChecks
{
    public static void ActionPlanAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709857879);
        var items = new List<ActionPlanAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ActionPlanAssignment.NewActionPlanAssignment($"action-plan-assignment-{index}", "sample"));
        }
        var report = ActionPlanAssignmentReport.BuildActionPlanAssignmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ActionPlanAssignmentReportCountsEachRecordOnce();
    }
}
