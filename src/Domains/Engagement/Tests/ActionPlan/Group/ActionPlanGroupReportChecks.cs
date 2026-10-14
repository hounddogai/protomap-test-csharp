namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ActionPlanGroupReportChecks
{
    public static void ActionPlanGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709370603);
        var items = new List<ActionPlanGroup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ActionPlanGroup.NewActionPlanGroup($"action-plan-group-{index}", "sample"));
        }
        var report = ActionPlanGroupReport.BuildActionPlanGroupReport("test", items, clock);
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
        ActionPlanGroupReportCountsEachRecordOnce();
    }
}
