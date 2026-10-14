namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanAdjustmentReportChecks
{
    public static void ActionPlanAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702303404);
        var items = new List<ActionPlanAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ActionPlanAdjustment.NewActionPlanAdjustment($"action-plan-adjustment-{index}", "sample"));
        }
        var report = ActionPlanAdjustmentReport.BuildActionPlanAdjustmentReport("test", items, clock);
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
        ActionPlanAdjustmentReportCountsEachRecordOnce();
    }
}
