namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanBundleReportChecks
{
    public static void ActionPlanBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707339285);
        var items = new List<ActionPlanBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ActionPlanBundle.NewActionPlanBundle($"action-plan-bundle-{index}", "sample"));
        }
        var report = ActionPlanBundleReport.BuildActionPlanBundleReport("test", items, clock);
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
        ActionPlanBundleReportCountsEachRecordOnce();
    }
}
